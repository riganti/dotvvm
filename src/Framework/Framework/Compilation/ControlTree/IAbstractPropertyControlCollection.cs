using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractPropertyControlCollection : IAbstractPropertySetter
    {
        IEnumerable<IAbstractControl> Controls { get; }
    }
}