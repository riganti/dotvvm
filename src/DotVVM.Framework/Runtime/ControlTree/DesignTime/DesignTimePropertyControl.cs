namespace DotVVM.Framework.Runtime.ControlTree.DesignTime
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