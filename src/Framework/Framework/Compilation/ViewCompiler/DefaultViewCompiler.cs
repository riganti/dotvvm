using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.Compilation.ViewCompiler
{
    public class DefaultViewCompiler : IViewCompiler
    {
        private readonly IControlTreeResolver controlTreeResolver;
        private readonly IBindingCompiler bindingCompiler;
        private readonly ViewCompilerConfiguration config;
        private readonly Func<Validation.ControlUsageValidationVisitor> controlValidatorFactory;
        private readonly CompositeDiagnosticsCompilationTracer tracer;
        private readonly ILogger<DefaultViewCompiler>? logger;

        public DefaultViewCompiler(IOptions<ViewCompilerConfiguration> config, IControlTreeResolver controlTreeResolver, IBindingCompiler bindingCompiler, Func<Validation.ControlUsageValidationVisitor> controlValidatorFactory, CompositeDiagnosticsCompilationTracer tracer, ILogger<DefaultViewCompiler>? logger = null)
        {
            this.config = config.Value;
            this.tracer = tracer;
            this.controlTreeResolver = controlTreeResolver;
            this.bindingCompiler = bindingCompiler;
            this.controlValidatorFactory = controlValidatorFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Compiles the view and returns a function that can be invoked repeatedly. The function builds full control tree and activates the page.
        /// </summary>
        protected virtual (ControlBuilderDescriptor, Func<Func<IControlBuilderFactory, IServiceProvider, DotvvmControl>>) CompileViewCore(string sourceCode, string fileName)
        {
            var tracingHandle = tracer.CompilationStarted(fileName, sourceCode);
            bool faultBlockHack(Exception e)
            {
                // avoids rethrowing exception and triggering the debugger by abusing
                // the `filter` block to report the error
                tracingHandle.Failed(e);
                return false;
            }
            try
            {
                // parse the document
                var tokenizer = new DothtmlTokenizer();
                tokenizer.Tokenize(sourceCode);
                var parser = new DothtmlParser();
                var node = parser.Parse(tokenizer.Tokens);
                tracingHandle.Parsed(tokenizer.Tokens, node);

                var resolvedView = (ResolvedTreeRoot)controlTreeResolver.ResolveTree(node, fileName);

                var descriptor = resolvedView.ControlBuilderDescriptor;

                return (descriptor, () => {
                    try
                    {
                        tracingHandle.Resolved(resolvedView, descriptor);

                        // avoid visiting invalid tree, it could trigger crashes in styles
                        CheckErrors(fileName, sourceCode, tracingHandle, tokenizer.Tokens, node, resolvedView);

                        foreach (var visitor in config.TreeVisitors)
                        {
                            var v = visitor();
                            try
                            {
                                resolvedView.Accept(v);
                                tracingHandle.AfterVisitor(v, resolvedView);
                            }
                            finally
                            {
                                (v as IDisposable)?.Dispose();
                            }
                        }

                        var validationVisitor = this.controlValidatorFactory.Invoke();
                        validationVisitor.WriteErrorsToNodes = false;
                        validationVisitor.DefaultVisit(resolvedView);

                        // validate tree again for new errors from the visitors and warnings
                        var diagnostics = CheckErrors(fileName, sourceCode, tracingHandle, tokenizer.Tokens, node, resolvedView, additionalDiagnostics: validationVisitor.Errors);
                        LogDiagnostics(tracingHandle, diagnostics, fileName, sourceCode);

                        var emitter = new DefaultViewCompilerCodeEmitter();
                        var compilingVisitor = new ViewCompilingVisitor(emitter, bindingCompiler);

                        resolvedView.Accept(compilingVisitor);

                        return compilingVisitor.BuildCompiledView;
                    }
                    catch (Exception e) when (faultBlockHack(e)) { throw; }
                    finally
                    {
                        (tracingHandle as IDisposable)?.Dispose();
                    }
                });
            }
            catch (Exception e) when (faultBlockHack(e)) { throw; }
        }

        private List<DotvvmCompilationDiagnostic> CheckErrors(string fileName, string sourceCode, IDiagnosticsCompilationTracer.Handle tracingHandle, List<DothtmlToken> tokens, DothtmlNode syntaxTree, ResolvedTreeRoot? resolvedTree, IEnumerable<DotvvmCompilationDiagnostic>? additionalDiagnostics = null)
        {
            var errorChecker = new ErrorCheckingVisitor(fileName);
            errorChecker.AddTokenizerErrors(tokens);
            errorChecker.AddSyntaxErrors(syntaxTree);
            resolvedTree?.Accept(errorChecker);

            if (additionalDiagnostics is { })
            {
                errorChecker.Diagnostics.AddRange(additionalDiagnostics);
            }

            if (DotvvmCompilationException.TryCreateFromDiagnostics(errorChecker.Diagnostics) is {} error)
            {
                LogDiagnostics(tracingHandle, error.AllDiagnostics, fileName, sourceCode);
                throw error;
            }
            return errorChecker.Diagnostics;
        }

        private void LogDiagnostics(IDiagnosticsCompilationTracer.Handle tracingHandle, IEnumerable<DotvvmCompilationDiagnostic> diagnostics, string fileName, string sourceCode)
        {
            var warnings = diagnostics.Where(d => d.IsWarning || d.IsError).ToArray();
            if (warnings.Length == 0) return;

            var lines = sourceCode.Split('\n');
            // Currently, all warnings are placed on syntax nodes (even when produced in control tree resolver)
            foreach (var warning in warnings)
            {
                var loc = warning.Location;
                var sourceLine = loc.LineNumber > 0 && loc.LineNumber <= lines.Length ? lines[loc.LineNumber.Value - 1] : null;
                sourceLine = sourceLine?.TrimEnd();

                var highlightLength = 1;
                if (sourceLine is {} && loc is { ColumnNumber: {}, LineErrorLength: > 0 })
                {
                    highlightLength = loc.LineErrorLength;
                    highlightLength = Math.Max(1, Math.Min(highlightLength, sourceLine.Length - loc.ColumnNumber.Value + 1));
                }

                var logEvent = new CompilationDiagnosticLogEvent(warning.Severity, warning.Message, fileName, loc.LineNumber, loc.ColumnNumber, sourceLine, highlightLength);
                logger?.Log(warning.IsWarning ? LogLevel.Warning : LogLevel.Error, 0, logEvent, null, (x, e) => x.ToString());

                tracingHandle.CompilationDiagnostic(warning, sourceLine);
            }
        }

        // custom log event implementing IEnumerable<KeyValuePair<string, object>> for Serilog properties
        private readonly struct CompilationDiagnosticLogEvent : IEnumerable<KeyValuePair<string, object?>>
        {
            public CompilationDiagnosticLogEvent(DiagnosticSeverity severity, string message, string? fileName, int? lineNumber, int? charPosition, string? contextLine, int highlightLength)
            {
                Severity = severity;
                Message = message;
                FileName = fileName;
                LineNumber = lineNumber;
                CharPosition = charPosition;
                ContextLine = contextLine;
                HighlightLength = highlightLength;
            }

            public DiagnosticSeverity Severity { get; }
            public string Message { get; }
            public string? FileName { get; }
            public int? LineNumber { get; }
            public int? CharPosition { get; }
            public string? ContextLine { get; }
            public int HighlightLength { get; }

            public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
            {
                yield return new("Message", Message);
                yield return new("FileName", FileName);
                yield return new("LineNumber", LineNumber);
                yield return new("CharPosition", CharPosition);
            }

            public override string ToString()
            {
                var fileLocation = (FileName ?? "UnknownFile") + (
                    LineNumber is {} && CharPosition is {} ? $"({LineNumber},{CharPosition + 1})" :
                    LineNumber is {} ? $":{LineNumber}" :
                    ""
                );
                string error;
                if (ContextLine is {} contextLine)
                {
                    var graphemeIndices = StringInfo.ParseCombiningCharacters(contextLine.Substring(0, Math.Min(contextLine.Length, CharPosition ?? 0)));
                    var padding = string.Concat(
                        graphemeIndices.Select(
                            startIndex => contextLine[startIndex] switch {
                                '\t' => "\t",
                                _ => " "
                            }
                        )
                    );
                    var errorHighlight = padding + new string('^', HighlightLength);
                    error = $"{fileLocation}: Dotvvm Compilation {Severity}\n{contextLine}\n{errorHighlight} {Message}";
                }
                else
                {
                    error = $"{fileLocation}: Dotvvm Compilation {Severity}: {Message}";
                }
                return error;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();
        }


        public virtual (ControlBuilderDescriptor, Func<IControlBuilder>) CompileView(string sourceCode, string fileName)
        {
            var (descriptor, viewBuildingDelegateGetter) = CompileViewCore(sourceCode, fileName);
            return (descriptor, () => new DelegateControlBuilder(descriptor, viewBuildingDelegateGetter()));
        }
    }
}
