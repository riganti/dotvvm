namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractTreeNode
    {

        IAbstractTreeNode Parent { get; }

        IAbstractTreeRoot TreeRoot { get; }
    }
}