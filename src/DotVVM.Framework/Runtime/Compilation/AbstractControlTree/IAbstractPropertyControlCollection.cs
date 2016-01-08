using System.Collections.Generic;

namespace DotVVM.Framework.Runtime.Compilation.AbstractControlTree
{
    public interface IAbstractPropertyControlCollection : IAbstractPropertySetter
    {
        IEnumerable<IAbstractControl> Controls { get; }
    }
}