#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    public static class DotvvmBindableObjectHelper
    {
        public static DotvvmProperty GetDotvvmProperty<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
        {
            var property = (prop.Body as MemberExpression)?.Member as PropertyInfo;
            if (property == null) throw new Exception($"Expression '{prop}' should be property access on the specified control.");
            return DotvvmProperty.ResolveProperty(property.DeclaringType!, property.Name) ?? throw new Exception($"Property '{property.DeclaringType!.Name}.{property.Name}' is not a registered DotvvmProperty.");
        }

        public static TControl SetBinding<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop, IBinding? binding)
            where TControl : DotvvmBindableObject
        {
            control.SetBinding(control.GetDotvvmProperty(prop), binding);
            return control;
        }

        public static TControl SetValue<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop, TProperty value)
            where TControl : DotvvmBindableObject
        {
            control.SetValue(control.GetDotvvmProperty(prop), value);
            return control;
        }

        public static IValueBinding<TProperty>? GetValueBinding<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
            => (IValueBinding<TProperty>?)control.GetValueBinding(control.GetDotvvmProperty(prop));

        public static ICommandBinding<TProperty>? GetCommandBinding<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
            => (ICommandBinding<TProperty>?)control.GetCommandBinding(control.GetDotvvmProperty(prop));

        [return: MaybeNull]
        public static TProperty GetValue<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
            => control.GetValue<TProperty>(control.GetDotvvmProperty(prop))!;

        internal static object? TryGeyValue(this DotvvmBindableObject control, DotvvmProperty property)
        {
            try
            {
                return control.GetValue(property);
            }
            catch
            {
                return property.DefaultValue;
            }
        }

        public static string DebugString(this DotvvmBindableObject control, DotvvmConfiguration? config = null, bool multiline = true)
        {
            if (control == null) return "null";

            config = config ?? (control.TryGeyValue(Internal.RequestContextProperty) as IDotvvmRequestContext)?.Configuration;

            var type = control.GetType();
            var properties = (from kvp in control.Properties
                              let p = kvp.Key
                              let rawValue = kvp.Value
                              where p.DeclaringType != typeof(Internal)
                              let isAttached = !p.DeclaringType.IsAssignableFrom(type)
                              orderby !isAttached, p.Name
                              let name = isAttached ? p.DeclaringType.Name + "." + p.Name : p.Name
                              let value = rawValue == null ? "<null>" :
                                          rawValue is ITemplate ? "<a template>" :
                                          rawValue is DotvvmBindableObject ? $"<control {rawValue.GetType()}>" :
                                          rawValue is IEnumerable<DotvvmBindableObject> controlCollection ? $"<{controlCollection.Count()} controls>" :
                                          rawValue is IEnumerable<object> collection ? string.Join(", ", collection) :
                                          rawValue.ToString()
                              let croppedValue = value.Length > 41 ? value.Substring(0, 40) + "…" : value
                              select new { p, name, croppedValue, value, isAttached }
                             ).ToArray();

            var location = (file: control.TryGeyValue(Internal.MarkupFileNameProperty) as string, line: control.TryGeyValue(Internal.MarkupLineNumberProperty) as int? ?? -1);
            var reg = config?.Markup.Controls.FirstOrDefault(c => c.Namespace == type.Namespace && Type.GetType(c.Namespace + "." + type.Name + ", " + c.Assembly) == type) ??
                      config?.Markup.Controls.FirstOrDefault(c => c.Namespace == type.Namespace) ??
                      config?.Markup.Controls.FirstOrDefault(c => c.Assembly == type.Assembly.GetName().Name);
            var ns = reg?.TagPrefix ?? "?";
            var tagName = type == typeof(HtmlGenericControl) ? ((HtmlGenericControl)control).TagName : ns + ":" + type.Name;

            var dothtmlString = $"<{tagName} ";
            var prefixLength = dothtmlString.Length;

            foreach (var p in properties)
            {
                if (multiline && p != properties[0])
                    dothtmlString += "\n" + new string(' ', prefixLength);
                dothtmlString += $"{p.name}={p.croppedValue}";
            }
            dothtmlString += "/>";

            var from = (location.file)
                     + (location.line >= 0 ? ":" + location.line : "");
            if (!String.IsNullOrWhiteSpace(from))
                from = (multiline ? "\n" : " ") + "from " + from.Trim();

            return dothtmlString + from;
        }
    }
}
