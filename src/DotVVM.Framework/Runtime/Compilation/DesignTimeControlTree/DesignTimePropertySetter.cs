using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.DesignTimeControlTree
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