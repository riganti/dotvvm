using System;

namespace DotVVM.AutoUI.Annotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SelectorAttribute : System.Attribute
{
    public Type PropertyType { get; }

    public SelectorAttribute(Type propertyType)
    {
        PropertyType = propertyType;
    }
}
