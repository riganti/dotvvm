using System;

namespace DotVVM.AutoUI.Annotations;

/// <summary>
/// Indicates that the user will select a value from a list of SelectionType provided by <see cref="ISelectionProvider{SelectionType}" />
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SelectionAttribute : System.Attribute
{
    /// <summary> The <see cref="Selection{TKey}" /> implementation </summary>
    public Type SelectionType { get; }

    public SelectionAttribute(Type selectionType)
    {
        if (!typeof(Selection).IsAssignableFrom(selectionType))
        {
            throw new ArgumentException($"The type {selectionType} used in SelectorAttribute must inherit from DotVVM.AutoUI.Annotations.Selection.");
        }

        SelectionType = selectionType;
    }
}
