#nullable enable
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface ITypeDescriptor
    {
        string Name { get; }

        string? Namespace { get; }

        string? Assembly { get; }

        string FullName { get; }

        bool IsAssignableTo(ITypeDescriptor typeDescriptor);

        bool IsAssignableFrom(ITypeDescriptor typeDescriptor);

        ControlMarkupOptionsAttribute? GetControlMarkupOptionsAttribute();

        bool IsEqualTo(ITypeDescriptor other);

        ITypeDescriptor? TryGetArrayElementOrIEnumerableType();

        ITypeDescriptor? TryGetPropertyType(string propertyName);

        ITypeDescriptor MakeGenericType(params ITypeDescriptor[] typeArguments);
    }
}
