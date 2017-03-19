using System;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation
{
    public class ErrorCheckingVisitor : ResolvedControlTreeVisitor
    {

        public override void VisitControl(ResolvedControl control)
        {
            if (control.DothtmlNode.HasNodeErrors)
            {
                throw new DotvvmCompilationException(string.Join("\r\n", control.DothtmlNode.NodeErrors), control.DothtmlNode.Tokens);
            }
            base.VisitControl(control);
        }

        public override void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding)
        {
            var errors = propertyBinding.Binding.Errors.Where(e => e.Item3 == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).ToArray();
            if (errors.Length > 0)
            {
                // TODO: better compilation error handling
                throw new DotvvmCompilationException($"Could not initialize binding '{propertyBinding.Binding.BindingType}', requirements {string.Join(", ", errors.Select(e => e.requirement))} was not met", new AggregateException(errors.Select(e => e.error)), propertyBinding.Binding.BindingNode.Tokens);
            }
            base.VisitPropertyBinding(propertyBinding);
        }

        public override void VisitView(ResolvedTreeRoot view)
        {
            if (view.DothtmlNode.HasNodeErrors)
            {
                throw new DotvvmCompilationException(string.Join("\r\n", view.DothtmlNode.NodeErrors), view.DothtmlNode.Tokens);
            }
            foreach (var directive in ((DothtmlRootNode) view.DothtmlNode).Directives)
            {
                if (directive.HasNodeErrors)
                {
                    throw new DotvvmCompilationException(string.Join("\r\n", directive.NodeErrors), directive.Tokens);
                }
            }
            base.VisitView(view);
        }
    }
}