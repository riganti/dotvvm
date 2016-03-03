using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IControlTreeResolver
    {
        IAbstractTreeRoot ResolveTree(DothtmlRootNode root, string fileName);
    }
}
