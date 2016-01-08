using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.DesignTimeControlTree
{
    public class DesignTimePropertyControl : DesignTimePropertySetter, IAbstractPropertyControl
    {
        public DesignTimePropertyControl(IPropertyDescriptor property, IAbstractControl control) : base(property)
        {
            Control = control;
        }

        public IAbstractControl Control { get; }
    }
}