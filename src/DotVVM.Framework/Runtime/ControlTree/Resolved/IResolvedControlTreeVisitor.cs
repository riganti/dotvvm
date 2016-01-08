namespace DotVVM.Framework.Runtime.ControlTree.Resolved
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
    }
}
