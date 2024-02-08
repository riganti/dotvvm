using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Compilation.ViewCompiler;

namespace DotVVM.Framework.Compilation
{
    /// <summary> Instrumets DotVVM view compilation, traced events are defined in <see cref="Handle" />.
    /// The tracers are found using IServiceProvider, to register your tracer, add it to DI with <c>service.AddSingleton&lt;IDiagnosticsCompilationTracer, MyTracer>()</c> </summary>
    public interface IDiagnosticsCompilationTracer
    {
        Handle CompilationStarted(string file, string sourceCode);
        /// <summary> Traces compilation of a single file, created in the <see cref="CompilationStarted(string, string)"/> method. Note that the class can also implement <see cref="IDisposable" />. </summary>
        abstract class Handle
        {
            /// <summary> Called after the DotHTML file is parsed and syntax tree is created. Called even when there are errors. </summary>
            public virtual void Parsed(List<DothtmlToken> tokens, DothtmlRootNode syntaxTree) { }
            /// <summary> Called after the entire tree has resolved types - controls have assigned type, attributes have assigned DotvvmProperty, bindings are compiled, ... </summary>
            public virtual void Resolved(ResolvedTreeRoot tree, ControlBuilderDescriptor descriptor) { }
            /// <summary> After initial resolving, the tree is post-processed using a number of visitors (<see cref="DataContextPropertyAssigningVisitor"/>, <see cref="Styles.StylingVisitor" />, <see cref="LiteralOptimizationVisitor" />, ...). After each visitor processing, this method is called. </summary>
            public virtual void AfterVisitor(ResolvedControlTreeVisitor visitor, ResolvedTreeRoot tree) { }
            /// <summary> For each compilation diagnostic (warning/error), this method is called. </summary>
            /// <param name="contextLine"> The line of code where the error occured. </param>
            public virtual void CompilationDiagnostic(DotvvmCompilationDiagnostic diagnostic, string? contextLine) { }
            /// <summary> Called if the compilation fails for any reason. Normally, <paramref name="exception"/> will be of type <see cref="DotvvmCompilationDiagnostic" /> </summary>
            public virtual void Failed(Exception exception) { }
        }
        /// <summary> Singleton tracing handle which does nothing. </summary>
        sealed class NopHandle: Handle
        {
            private NopHandle() { }
            public static readonly NopHandle Instance = new NopHandle();
        }
    }

    public sealed class CompositeDiagnosticsCompilationTracer : IDiagnosticsCompilationTracer
    {
        readonly IDiagnosticsCompilationTracer[] tracers;

        public CompositeDiagnosticsCompilationTracer(IEnumerable<IDiagnosticsCompilationTracer> tracers)
        {
            this.tracers = tracers.ToArray();
        }

        public IDiagnosticsCompilationTracer.Handle CompilationStarted(string file, string sourceCode)
        {
            var handles = this.tracers
                              .Select(t => t.CompilationStarted(file, sourceCode))
                              .Where(t => t != IDiagnosticsCompilationTracer.NopHandle.Instance)
                              .ToArray();

            
            return handles.Length switch {
                0 => IDiagnosticsCompilationTracer.NopHandle.Instance,
                1 => handles[0],
                _ => new Handle(handles)
            };
        }

        sealed class Handle : IDiagnosticsCompilationTracer.Handle, IDisposable
        {
            private IDiagnosticsCompilationTracer.Handle[] handles;

            public Handle(IDiagnosticsCompilationTracer.Handle[] handles)
            {
                this.handles = handles;
            }

            public override void AfterVisitor(ResolvedControlTreeVisitor visitor, ResolvedTreeRoot tree)
            {
                foreach (var h in handles)
                    h.AfterVisitor(visitor, tree);
            }
            public override void CompilationDiagnostic(DotvvmCompilationDiagnostic warning, string? contextLine)
            {
                foreach (var h in handles)
                    h.CompilationDiagnostic(warning, contextLine);
            }


            public override void Failed(Exception exception)
            {
                foreach (var h in handles)
                    h.Failed(exception);
            }
            public override void Parsed(List<DothtmlToken> tokens, DothtmlRootNode syntaxTree)
            {
                foreach (var h in handles)
                    h.Parsed(tokens, syntaxTree);
            }
            public override void Resolved(ResolvedTreeRoot tree, ControlBuilderDescriptor descriptor)
            {
                foreach (var h in handles)
                    h.Resolved(tree, descriptor);
            }
            public void Dispose()
            {
                foreach (var h in handles)
                    (h as IDisposable)?.Dispose();
            }
        }
    }
}
