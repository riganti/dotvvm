using System;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation
{
    public class ErrorCheckingVisitor : ResolvedControlTreeVisitor
    {

        public override void VisitControl(ResolvedControl control)
        {
            if (control.DothtmlNode is { HasNodeErrors: true })
            {
                throw new DotvvmCompilationException(string.Join("\r\n", control.DothtmlNode.NodeErrors), control.DothtmlNode.Tokens);
            }
            base.VisitControl(control);
        }

        public override void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding)
        {
            var errors = propertyBinding.Binding.Errors;
            if (errors.HasErrors)
            {
                // TODO: aggregate all errors from the page
                throw new DotvvmCompilationException(
                    errors.GetErrorMessage(propertyBinding.Binding.Binding),
                    errors.Exceptions.Count() > 1 ? new AggregateException(errors.Exceptions) : errors.Exceptions.SingleOrDefault(),
                    propertyBinding.Binding.BindingNode?.Tokens);
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
