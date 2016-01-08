using System.Collections.Generic;
using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.DesignTimeControlTree
{
    public class DesignTimePropertyControlCollection : DesignTimePropertySetter, IAbstractPropertyControlCollection
    {
        public DesignTimePropertyControlCollection(IPropertyDescriptor property, IEnumerable<IAbstractControl> controls) : base(property)
        {
            Controls = controls;
        }

        public IEnumerable<IAbstractControl> Controls { get; }
    }
}