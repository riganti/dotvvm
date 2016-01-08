using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Runtime.ControlTree.Resolved
{
    public abstract class ResolvedPropertySetter : IResolvedTreeNode, IAbstractPropertySetter
    {
        public DotvvmProperty Property { get; set; }

        IPropertyDescriptor IAbstractPropertySetter.Property => Property;

        public ResolvedPropertySetter(DotvvmProperty property)
        {
            Property = property;
        }

        public abstract void Accept(IResolvedControlTreeVisitor visitor);

        public abstract void AcceptChildren(IResolvedControlTreeVisitor visitor);
    }
}
