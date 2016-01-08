namespace DotVVM.Framework.Runtime.ControlTree.DesignTime
{
    public class DesignTimePropertyBinding : DesignTimePropertySetter, IAbstractPropertyBinding
    {
        public DesignTimePropertyBinding(IPropertyDescriptor property, IAbstractBinding binding) : base(property)
        {
            Binding = binding;
        }

        public IAbstractBinding Binding { get; }
    }
}