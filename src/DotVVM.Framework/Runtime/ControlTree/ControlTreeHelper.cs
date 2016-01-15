using DotVVM.Framework.Parser.Dothtml.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.ControlTree
{
    public static class ControlTreeHelper
    {
        public static bool HasEmptyContent(this IAbstractControl control)
            => control.Content.All(c => !c.DothtmlNode.IsNotEmpty()); // allow only whitespace literals

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
