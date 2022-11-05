using System;

namespace DotVVM.Framework.Configuration;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class CustomPrimitiveTypeAttribute : Attribute
{
    /// <summary>
    /// Gets a type which will be used for this custom type on the client side. Only types from ReflectionUtils.PrimitiveTypes (or their nullable versions) are supported.
    /// </summary>
    public Type ClientSideType { get; }

    /// <summary>
    /// Gets a type implementing ICustomPrimitiveTypeConverter which converts the value from string or other type that may appear in route parameters collection.
    /// </summary>
    public Type ConverterType { get; }

    public CustomPrimitiveTypeAttribute(Type clientSideType, Type converterType)
    {
        // types are validated later as we don't know on which type the attribute is applied so the error message wouldn't tell the user where is the problem
        ClientSideType = clientSideType;
        ConverterType = converterType;
    }

}
