using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractPropertyTemplate : IAbstractPropertySetter
    {
        IEnumerable<IAbstractControl> Content { get; }
    }
}