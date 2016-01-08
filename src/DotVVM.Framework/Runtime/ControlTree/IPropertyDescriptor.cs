using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IPropertyDescriptor
    {

        string Name { get; }

        ITypeDescriptor DeclaringType { get; }

        ITypeDescriptor PropertyType { get; }

        object DefaultValue { get; }

        MarkupOptionsAttribute MarkupOptions { get; }

        bool IsValueInherited { get; }

        bool IsVirtual { get; }

        bool IsBindingProperty { get; }
        string FullName { get; }
    }
}