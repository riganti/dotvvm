using System;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;

namespace DotVVM.Framework.Binding;

public class ExtractGenericArgumentDataContextChangeAttribute : DataContextChangeAttribute
{
    public ITypeDescriptor GenericType { get; }
    public int TypeArgumentIndex { get; }
    public override int Order { get; }

    public ExtractGenericArgumentDataContextChangeAttribute(Type genericType, int typeArgumentIndex, int order = 0)
        : this(new ResolvedTypeDescriptor(genericType), typeArgumentIndex, order)
    {
    }
   

    public ExtractGenericArgumentDataContextChangeAttribute(ITypeDescriptor genericType, int typeArgumentIndex, int order = 0)
    {
        if (!genericType.IsGenericTypeDefinition)
        {
            throw new ArgumentException($"The {nameof(genericType)} argument must be a generic type definition!", nameof(genericType));
        }
        if (typeArgumentIndex < 0 || typeArgumentIndex >= genericType.GetGenericArguments()!.Length)
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
            throw new Exception($"The data context {dataContext.CSharpFullName} doesn't implement {GenericType.CSharpFullName}!");
        }
        else if (implementations.Count > 1)
        {
            throw new Exception($"The data context {dataContext.CSharpFullName} has multiple implementations of {GenericType.CSharpFullName}! Cannot decide which one to extract:\n" + string.Join("\n", implementations.Select(t => t.CSharpFullName)));
        }
        return implementations[0].GetGenericArguments()![TypeArgumentIndex];
    }

    public override Type? GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, DotvvmBindableObject control, DotvvmProperty? property = null)
    {
        var implementations = ReflectionUtils.GetBaseTypesAndInterfaces(dataContext)
            .Where(i => i.IsGenericType && new ResolvedTypeDescriptor(i.GetGenericTypeDefinition()).IsEqualTo(GenericType))
            .ToList();
        if (implementations.Count == 0)
        {
            throw new Exception($"The data context {dataContext.ToCode()} doesn't implement {GenericType.CSharpFullName}!");
        }
        else if (implementations.Count > 1)
        {
            throw new Exception($"The data context {dataContext.ToCode()} has multiple implementations of {GenericType.CSharpFullName}! Cannot decide which one to extract:\n" + string.Join("\n", implementations.Select(t => t.ToCode())));
        }
        return implementations[0].GetGenericArguments()[TypeArgumentIndex];
    }
}
