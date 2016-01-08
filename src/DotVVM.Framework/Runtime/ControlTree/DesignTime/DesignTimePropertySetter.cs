namespace DotVVM.Framework.Runtime.ControlTree.DesignTime
{
    public abstract class DesignTimePropertySetter : IAbstractPropertySetter
    {
        public IPropertyDescriptor Property { get; }

        public DesignTimePropertySetter(IPropertyDescriptor property)
        {
            Property = property;
        }
    }
}