namespace DotVVM.Framework.Runtime.ControlTree.DesignTime
{
    public class DesignTimePropertyValue : DesignTimePropertySetter, IAbstractPropertyValue
    {
        public DesignTimePropertyValue(IPropertyDescriptor property, object value) : base(property)
        {
            Value = value;
        }

        public object Value { get; }
    }
}