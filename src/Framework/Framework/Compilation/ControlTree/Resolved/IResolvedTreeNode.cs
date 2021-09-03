
namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public interface IResolvedTreeNode
    {
        void Accept(IResolvedControlTreeVisitor visitor);

        void AcceptChildren(IResolvedControlTreeVisitor visitor);
    }
}
