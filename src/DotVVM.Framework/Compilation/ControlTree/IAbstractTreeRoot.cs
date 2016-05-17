using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractTreeRoot : IAbstractContentNode
    {

        Dictionary<string, string> Directives { get; }
        
    }
}
