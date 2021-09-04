using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public abstract class ResolvedTreeNode : IAbstractTreeNode, IResolvedTreeNode
    {
        public virtual DothtmlNode? DothtmlNode { get; set; }

        public ResolvedTreeNode? Parent { get; set; }

        private ResolvedTreeRoot? treeRoot;
        public ResolvedTreeRoot TreeRoot => treeRoot ?? (treeRoot = Parent?.TreeRoot ?? this as ResolvedTreeRoot)!;

        public IEnumerable<ResolvedTreeNode> GetAncestors(bool includeThis = true)
        {
            var p = includeThis ? this : Parent;
            while (p != null)
            {
                yield return p;
                p = p.Parent;
            }
        }

        IAbstractTreeNode? IAbstractTreeNode.Parent => Parent;
        IAbstractTreeRoot IAbstractTreeNode.TreeRoot => TreeRoot;


        public abstract void Accept(IResolvedControlTreeVisitor visitor);

        public abstract void AcceptChildren(IResolvedControlTreeVisitor visitor);
    }
}
