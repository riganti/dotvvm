using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Compilation.ViewCompiler;

namespace DotVVM.Framework.Compilation
{

    public interface IDiagnosticsCompilationTracer
    {
        Handle CompilationStarted(string file, string sourceCode);
        abstract class Handle
        {
            public virtual void Parsed(List<DothtmlToken> tokens, DothtmlRootNode syntaxTree) { }
            public virtual void Resolved(ResolvedTreeRoot tree, ControlBuilderDescriptor descriptor) { }
            public virtual void AfterVisitor(ResolvedControlTreeVisitor visitor, ResolvedTreeRoot tree) { }
            public virtual void CompilationDiagnostic(DotvvmCompilationDiagnostic diagnostic, string? contextLine) { }
            public virtual void Failed(Exception exception) { }
        }
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
