using System;

namespace DotVVM.AutoUI.Annotations;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SelectionAttribute : System.Attribute
{
    public Type PropertyType { get; }

    public SelectionAttribute(Type propertyType)
    {
        PropertyType = propertyType;
    }
}
