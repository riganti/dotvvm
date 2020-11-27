#nullable enable
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public abstract class ResolvedPropertySetter : ResolvedTreeNode, IAbstractPropertySetter
    {
        public DotvvmProperty Property { get; set; }

        IPropertyDescriptor IAbstractPropertySetter.Property => Property;

        public ResolvedPropertySetter(DotvvmProperty property)
        {
            Property = property;
        }
    }
}
