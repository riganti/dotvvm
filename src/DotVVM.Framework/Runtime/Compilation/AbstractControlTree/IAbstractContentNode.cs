using System.Collections.Generic;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Runtime.Compilation.AbstractControlTree
{
    public interface IAbstractContentNode
    {

        DothtmlNode DothtmlNode { get; }
        IEnumerable<IAbstractControl> Content { get; }
        IControlResolverMetadata Metadata { get; }
        IDataContextStack DataContextTypeStack { get; }


    }
}