using System.Collections.Generic;

namespace DotVVM.Framework.Runtime.Compilation.AbstractControlTree
{
    public interface IAbstractPropertyTemplate : IAbstractPropertySetter
    {
        IEnumerable<IAbstractControl> Content { get; }
    }
}