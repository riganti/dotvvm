using System;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding;

public class ExtractGenericArgumentDataContextChangeAttribute : DataContextChangeAttribute
{
    public Type GenericType { get; }
    public int TypeArgumentIndex { get; }
    public override int Order { get; }

    public ExtractGenericArgumentDataContextChangeAttribute(Type genericType, int typeArgumentIndex, int order = 0)
    {
        if (!genericType.IsGenericTypeDefinition)
        {
            throw new ArgumentException($"The {nameof(genericType)} argument must be a generic type definition!", nameof(genericType));
        }
        if (typeArgumentIndex < 0 || typeArgumentIndex >= genericType.GetGenericArguments().Length)
        {
            throw new ArgumentOutOfRangeException(nameof(typeArgumentIndex), $"The {nameof(typeArgumentIndex)} is not a valid index of a type argument!");
        }

        GenericType = genericType;
        TypeArgumentIndex = typeArgumentIndex;
        Order = order;
    }

    public override ITypeDescriptor? GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor? property = null)
    {
        var implementations = dataContext.FindGenericImplementations(GenericType).ToList();
        if (implementations.Count == 0)
        {
            throw new Exception($"The data context {dataContext.CSharpFullName} doesn't implement {GenericType}!");
        }
        else if (implementations.Count > 1)
        {
            throw new Exception($"The data context {dataContext.CSharpFullName} has multiple implementations of {GenericType}! Cannot decide which one to extract:\n" + string.Join("\n", implementations.Select(t => t.CSharpFullName)));
        }
        return implementations[0].GetGenericArguments()![TypeArgumentIndex];
    }

    public override Type? GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, DotvvmBindableObject control, DotvvmProperty? property = null)
    {
        var implementations = ReflectionUtils.GetBaseTypesAndInterfaces(dataContext).ToList();
        if (implementations.Count == 0)
        {
            throw new Exception($"The data context {dataContext} doesn't implement {GenericType}!");
        }
        else if (implementations.Count > 1)
        {
            throw new Exception($"The data context {dataContext} has multiple implementations of {GenericType}! Cannot decide which one to extract:\n" + string.Join("\n", implementations));
        }
        return implementations[0].GetGenericArguments()[TypeArgumentIndex];
    }
}
