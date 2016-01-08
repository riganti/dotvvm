using System.Collections.Generic;

namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IAbstractTreeRoot : IAbstractContentNode
    {

        Dictionary<string, string> Directives { get; }
        
    }
}
