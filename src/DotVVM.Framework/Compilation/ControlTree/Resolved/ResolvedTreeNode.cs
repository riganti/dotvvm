namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public abstract class ResolvedTreeNode : IAbstractTreeNode, IResolvedTreeNode
    {
        public ResolvedTreeNode Parent { get; set; }

        IAbstractTreeNode IAbstractTreeNode.Parent => Parent;
        

        public abstract void Accept(IResolvedControlTreeVisitor visitor);

        public abstract void AcceptChildren(IResolvedControlTreeVisitor visitor);
    }
}