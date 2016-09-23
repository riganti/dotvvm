namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public interface IResolvedControlTreeVisitor
    {
        void VisitControl(ResolvedControl control);
        void VisitView(ResolvedTreeRoot view);
        void VisitPropertyValue(ResolvedPropertyValue propertyValue);
        void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding);
        void VisitPropertyTemplate(ResolvedPropertyTemplate propertyTemplate);
        void VisitPropertyControlCollection(ResolvedPropertyControlCollection propertyControlCollection);
        void VisitPropertyControl(ResolvedPropertyControl propertyControl);
        void VisitBinding(ResolvedBinding binding);
        void VisitDirective(ResolvedDirective directive);
        void VisitImportDirective(ResolvedImportDirective importDirective);
    }
}
