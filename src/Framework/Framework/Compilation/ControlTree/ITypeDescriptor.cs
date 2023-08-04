using System;
using System.Collections;
using System.Collections.Generic;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface ITypeDescriptor
    {
        string Name { get; }

        string? Namespace { get; }

        string? Assembly { get; }

        string FullName { get; }
        /// <summary> Returns type name with generic arguments in the C# style. Does not include namespaces. </summary>
        string CSharpName { get; }
        /// <summary> Returns type name including namespace with generic arguments in the C# style. </summary>
        string CSharpFullName { get; }

        bool IsAssignableTo(ITypeDescriptor typeDescriptor);

        bool IsAssignableFrom(ITypeDescriptor typeDescriptor);

        ControlMarkupOptionsAttribute? GetControlMarkupOptionsAttribute();

        bool IsEqualTo(ITypeDescriptor other);
        bool IsEqualTo(Type other);

        ITypeDescriptor? TryGetArrayElementOrIEnumerableType();

        ITypeDescriptor? TryGetPropertyType(string propertyName);

        ITypeDescriptor MakeGenericType(params ITypeDescriptor[] typeArguments);

        IEnumerable<ITypeDescriptor> FindGenericImplementations(ITypeDescriptor genericType);

        ITypeDescriptor[]? GetGenericArguments();
    }
}
