namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public abstract class ResolvedTreeNode : IAbstractTreeNode, IResolvedTreeNode
    {
        public ResolvedTreeNode Parent { get; set; }

        private ResolvedTreeRoot treeRoot;
        public ResolvedTreeRoot TreeRoot => treeRoot ?? (treeRoot = Parent?.TreeRoot ?? this as ResolvedTreeRoot);

        IAbstractTreeNode IAbstractTreeNode.Parent => Parent;
        IAbstractTreeRoot IAbstractTreeNode.TreeRoot => TreeRoot;


        public abstract void Accept(IResolvedControlTreeVisitor visitor);

        public abstract void AcceptChildren(IResolvedControlTreeVisitor visitor);
    }
}