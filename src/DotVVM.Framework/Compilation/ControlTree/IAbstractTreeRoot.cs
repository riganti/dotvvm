using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractTreeRoot : IAbstractContentNode
    {

        Dictionary<string, List<string>> Directives { get; }
        
    }
}
