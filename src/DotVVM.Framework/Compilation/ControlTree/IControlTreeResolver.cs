#nullable enable
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IControlTreeResolver
    {
        IAbstractTreeRoot ResolveTree(DothtmlRootNode root, MarkupFile fileName);
    }
}
