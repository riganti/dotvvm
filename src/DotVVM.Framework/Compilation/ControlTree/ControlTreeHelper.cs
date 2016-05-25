using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public static class ControlTreeHelper
    {
        public static bool HasEmptyContent(this IAbstractControl control)
            => control.Content.All(c => !DothtmlNodeHelper.IsNotEmpty(c.DothtmlNode)); // allow only whitespace literals

        public static bool HasProperty(this IAbstractControl control, IPropertyDescriptor property)
        {
            IAbstractPropertySetter blackHole;
            return control.TryGetProperty(property, out blackHole);
        }

        public static bool HasPropertyValue(this IAbstractControl control, IPropertyDescriptor property)
        {
            IAbstractPropertySetter setter;
            return control.TryGetProperty(property, out setter) && setter is IAbstractPropertyValue;
        }
    }
}
