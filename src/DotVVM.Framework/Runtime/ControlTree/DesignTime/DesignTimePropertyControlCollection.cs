using System.Collections.Generic;

namespace DotVVM.Framework.Runtime.ControlTree.DesignTime
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