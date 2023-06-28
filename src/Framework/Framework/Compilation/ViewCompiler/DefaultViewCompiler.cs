using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ILogger<DefaultViewCompiler>? logger;

        public DefaultViewCompiler(IOptions<ViewCompilerConfiguration> config, IControlTreeResolver controlTreeResolver, IBindingCompiler bindingCompiler, Func<Validation.ControlUsageValidationVisitor> controlValidatorFactory, ILogger<DefaultViewCompiler>? logger = null)
        {
            this.config = config.Value;
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
            // parse the document
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(sourceCode);
            var parser = new DothtmlParser();
            var node = parser.Parse(tokenizer.Tokens);

            var resolvedView = (ResolvedTreeRoot)controlTreeResolver.ResolveTree(node, fileName);

            var descriptor = resolvedView.ControlBuilderDescriptor;

            return (descriptor, () => {

                var errorCheckingVisitor = new ErrorCheckingVisitor();
                resolvedView.Accept(errorCheckingVisitor);

                foreach (var token in tokenizer.Tokens)
                {
                    if (token.Error is { IsCritical: true })
                    {
                        throw new DotvvmCompilationException(token.Error.ErrorMessage, new[] { (token.Error as BeginWithLastTokenOfTypeTokenError<DothtmlToken, DothtmlTokenType>)?.LastToken ?? token });
                    }
                }

                foreach (var n in node.EnumerateNodes())
                {
                    if (n.HasNodeErrors)
                    {
                        throw new DotvvmCompilationException(string.Join(", ", n.NodeErrors), n.Tokens);
                    }
                }

                foreach (var visitor in config.TreeVisitors)
                    visitor().ApplyAction(resolvedView.Accept).ApplyAction(v => (v as IDisposable)?.Dispose());


                var validationVisitor = this.controlValidatorFactory.Invoke();
                validationVisitor.VisitAndAssert(resolvedView);
                LogWarnings(resolvedView, sourceCode);

                var emitter = new DefaultViewCompilerCodeEmitter();
                var compilingVisitor = new ViewCompilingVisitor(emitter, bindingCompiler);

                resolvedView.Accept(compilingVisitor);

                return compilingVisitor.BuildCompiledView;
            });
        }

        private void LogWarnings(ResolvedTreeRoot resolvedView, string sourceCode)
        {
            string[]? lines = null;
            if (logger is null || resolvedView.DothtmlNode is null) return;
            // Currently, all warnings are placed on syntax nodes (even when produced in control tree resolver)
            foreach (var node in resolvedView.DothtmlNode.EnumerateNodes())
            {
                if (node.HasNodeWarnings)
                {
                    lines ??= sourceCode.Split('\n');
                    var nodePosition = node.Tokens.FirstOrDefault();
                    var sourceLine = nodePosition is { LineNumber: > 0 } && nodePosition.LineNumber <= lines.Length ? lines[nodePosition.LineNumber - 1] : null;
                    sourceLine = sourceLine?.TrimEnd();
                    var highlightLength = 1;
                    if (sourceLine is {} && nodePosition is {})
                    {
                        highlightLength = node.Tokens.Where(t => t.LineNumber == nodePosition?.LineNumber).Sum(t => t.Length);
                        highlightLength = Math.Max(1, Math.Min(highlightLength, sourceLine.Length - nodePosition.ColumnNumber + 1));
                    }

                    foreach (var warning in node.NodeWarnings)
                    {
                        var logEvent = new CompilationWarning(warning, resolvedView.FileName, nodePosition?.LineNumber, nodePosition?.ColumnNumber, sourceLine, highlightLength);
                        logger.Log(LogLevel.Warning, 0, logEvent, null, (x, e) => x.ToString());
                    }
                }
            }
        }

        // custom log event implementing IEnumerable<KeyValuePair<string, object>> for Serilog properties
        private readonly struct CompilationWarning : IEnumerable<KeyValuePair<string, object?>>
        {
            public CompilationWarning(string message, string? fileName, int? lineNumber, int? charPosition, string? contextLine, int highlightLength)
            {
                Message = message;
                FileName = fileName;
                LineNumber = lineNumber;
                CharPosition = charPosition;
                ContextLine = contextLine;
                HighlightLength = highlightLength;
            }

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
                if (ContextLine is {})
                {
                    var errorHighlight = new string(' ', CharPosition ?? 1) + new string('^', HighlightLength);
                    error = $"{fileLocation}: Dotvvm Compilation Warning\n{ContextLine}\n{errorHighlight} {Message}";
                }
                else
                {
                    error = $"{fileLocation}: Dotvvm Compilation Warning: {Message}";
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
