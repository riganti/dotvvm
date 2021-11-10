using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation
{
    /// <summary>
    /// Contains debug information about original binding location.
    /// </summary>
    public sealed record DotvvmLocationInfo(
        string? FileName,
        (int start, int end)[]? Ranges,
        int? LineNumber,
        Type? ControlType,
        DotvvmProperty? RelatedProperty = null
    ) {
        public static DotvvmLocationInfo FromControl(DotvvmBindableObject obj)
        {
            var lineNumber = (int?)Internal.MarkupLineNumberProperty.GetValue(obj);
            if (obj.Parent != null) obj = obj.Parent;
            var fileName = (string?)Internal.MarkupFileNameProperty.GetValue(obj);
            return new DotvvmLocationInfo(
                fileName,
                null,
                lineNumber,
                obj.GetType()
            );
        }
    }
}
