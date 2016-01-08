using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.DesignTimeControlTree
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