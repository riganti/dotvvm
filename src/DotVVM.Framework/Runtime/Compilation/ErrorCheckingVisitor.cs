using DotVVM.Framework.Exceptions;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.ControlTree.Resolved;

namespace DotVVM.Framework.Runtime.Compilation
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
            if(propertyBinding.Binding.ParsingError != null)
            {
                throw new DotvvmCompilationException("Binding compilation failed", propertyBinding.Binding.ParsingError, propertyBinding.Binding.BindingNode.Tokens);
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