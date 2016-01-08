using System.Collections.Generic;

namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IAbstractPropertyTemplate : IAbstractPropertySetter
    {
        IEnumerable<IAbstractControl> Content { get; }
    }
}