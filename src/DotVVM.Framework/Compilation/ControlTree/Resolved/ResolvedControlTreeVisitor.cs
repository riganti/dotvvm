using System;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public abstract class ResolvedControlTreeVisitor : IResolvedControlTreeVisitor
    {
        public virtual void VisitControl(ResolvedControl control)
        {
            DefaultVisit(control);
        }

        public virtual void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding)
        {
            DefaultVisit(propertyBinding);
        }

        public virtual void VisitPropertyValue(ResolvedPropertyValue propertyValue)
        {
            DefaultVisit(propertyValue);
        }

        public virtual void VisitView(ResolvedTreeRoot view)
        {
            DefaultVisit(view);
        }

        public virtual void VisitPropertyTemplate(ResolvedPropertyTemplate propertyTemplate)
        {
            DefaultVisit(propertyTemplate);
        }

        public virtual void VisitPropertyControlCollection(ResolvedPropertyControlCollection propertyControlCollection)
        {
            DefaultVisit(propertyControlCollection);
        }

        public virtual void VisitPropertyControl(ResolvedPropertyControl propertyControl)
        {
            DefaultVisit(propertyControl);
        }

        public virtual void VisitBinding(ResolvedBinding binding)
        {
            DefaultVisit(binding);
        }

        public virtual void VisitDirective(ResolvedDirective directive)
        {
            DefaultVisit(directive);
        }

        public virtual void DefaultVisit(IResolvedTreeNode node)
        {
            node.AcceptChildren(this);
        }

        public void VisitImportDirective(ResolvedImportDirective importDirective)
        {
            DefaultVisit(importDirective);
        }
    }
}
