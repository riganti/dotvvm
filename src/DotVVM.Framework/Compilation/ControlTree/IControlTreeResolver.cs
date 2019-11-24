#nullable enable
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IControlTreeResolver
    {
        IAbstractTreeRoot ResolveTree(DothtmlRootNode root, string fileName);
    }
}
