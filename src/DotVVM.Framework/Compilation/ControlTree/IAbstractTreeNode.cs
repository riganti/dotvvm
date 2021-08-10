#nullable enable

using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractTreeNode
    {
        DothtmlNode? DothtmlNode { get; }

        IAbstractTreeNode? Parent { get; }

        IAbstractTreeRoot TreeRoot { get; }
    }
}
