using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.DesignTimeControlTree
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