using System.Collections.Generic;

namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IAbstractPropertyControlCollection : IAbstractPropertySetter
    {
        IEnumerable<IAbstractControl> Controls { get; }
    }
}