using System;
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

        public static IAbstractPropertySetter GetHtmlAttribute(this IAbstractControl control, string memberName) =>
            GetPropertyGroupMember(control, "", memberName);
        public static IAbstractPropertySetter GetPropertyGroupMember(this IAbstractControl control, string prefix, string memberName)
        {
            IAbstractPropertySetter value;
            control.TryGetProperty(control.Metadata.GetPropertyGroupMember(prefix, memberName), out value);
            return value;
        }

        public static IPropertyDescriptor GetHtmlAttributeDescriptor(this IControlResolverMetadata metadata, string name)
            => metadata.GetPropertyGroupMember("", name);
        public static IPropertyDescriptor GetPropertyGroupMember(this IControlResolverMetadata metadata, string prefix, string name)
        {
            var group = metadata.PropertyGroups.FirstOrDefault(f => f.Prefix == prefix).PropertyGroup;
            if (group == null) throw new NotSupportedException($"Control { metadata.Type.Name } does not support property group with prefix '{prefix}'.");
            return group.GetDotvvmProperty(name);
        }
    }
}
