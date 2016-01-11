namespace DotVVM.Framework.Runtime.ControlTree.Resolved
{
    public interface IResolvedTreeNode
    {
        void Accept(IResolvedControlTreeVisitor visitor);

        void AcceptChildren(IResolvedControlTreeVisitor visitor);
    }
}
