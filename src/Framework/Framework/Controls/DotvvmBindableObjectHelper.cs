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
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    public static class DotvvmBindableObjectHelper
    {
        /// <summary> Gets the DotvvmProperty referenced by the lambda expression. </summary>
        public static DotvvmProperty GetDotvvmProperty<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject =>
            ReflectionUtils.GetDotvvmPropertyFromExpression(prop);
        /// <summary> Gets the DotvvmProperty with the specified name.  </summary>
        public static DotvvmProperty GetDotvvmProperty<TControl>(this TControl control, string propName)
            where TControl : DotvvmBindableObject
        {
            return DotvvmProperty.ResolveProperty(typeof(TControl), propName) ?? throw new Exception($"Property '{typeof(TControl)}.{propName}' is not a registered DotvvmProperty.");
        }

        /// <summary> Sets binding into the DotvvmProperty referenced in the lambda expression. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetProperty<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop, IBinding? binding)
            where TControl : DotvvmBindableObject
        {
            control.SetBinding(control.GetDotvvmProperty(prop), binding);
            return control;
        }

        /// <summary> Sets binding into the DotvvmProperty. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetProperty<TControl>(this TControl control, DotvvmProperty prop, IBinding? binding)
            where TControl : DotvvmBindableObject
        {
            control.SetBinding(prop, binding);
            return control;
        }
        /// <summary> Sets binding into the DotvvmProperty with specified name. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetProperty<TControl>(this TControl control, string propName, IBinding? binding)
            where TControl : DotvvmBindableObject
        {
            control.SetBinding(control.GetDotvvmProperty(propName), binding);
            return control;
        }

        /// <summary> Sets value or binding into the DotvvmProperty referenced in the lambda expression. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetProperty<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop, ValueOrBinding<TProperty> value)
            where TControl : DotvvmBindableObject
        {
            control.SetValue(control.GetDotvvmProperty(prop), value);
            return control;
        }
        /// <summary> Sets value or binding into the DotvvmProperty with specified name. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetProperty<TControl, TProperty>(this TControl control, string propName, ValueOrBinding<TProperty> valueOrBinding)
            where TControl : DotvvmBindableObject
        {
            control.SetProperty(control.GetDotvvmProperty(propName), valueOrBinding);
            return control;
        }

        /// <summary> Sets value or binding into the DotvvmProperty. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetProperty<TControl, TProperty>(this TControl control, DotvvmProperty property, ValueOrBinding<TProperty> valueOrBinding)
            where TControl: DotvvmBindableObject
        {
            if (valueOrBinding.BindingOrDefault == null)
                control.SetValue(property, valueOrBinding.BoxedValue);
            else
                control.SetBinding(property, valueOrBinding.BindingOrDefault);
            return control;
        }

        /// <summary> Sets value or binding into the DotvvmProperty. If the <paramref name="valueOrBinding"/> is null, the property is removed. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetProperty<TControl, TProperty>(this TControl control, DotvvmProperty property, ValueOrBinding<TProperty>? valueOrBinding)
            where TControl: DotvvmBindableObject
        {
            if (valueOrBinding.HasValue)
                control.SetProperty(property, valueOrBinding.GetValueOrDefault());
            else
                control.Properties.Remove(property);
            return control;
        }

        [Obsolete("Please prefer the SetProperty method with the same signature")]
        public static TControl SetBinding<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop, IBinding? binding)
            where TControl : DotvvmBindableObject =>
            control.SetProperty(prop, binding);
        [Obsolete("Please prefer the SetProperty method with the same signature")]
        public static TControl SetValue<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop, TProperty value)
            where TControl : DotvvmBindableObject =>
            control.SetProperty(prop, value);

        /// <summary> Sets a binding into member of the specified property group. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetProperty<TControl, TProperty>(this TControl control, Func<TControl, VirtualPropertyGroupDictionary<TProperty>> prop, string key, IBinding? binding)
            where TControl : DotvvmBindableObject
        {
            var d = prop(control);
            d.AddBinding(key, binding);
            return control;
        }

        /// <summary> Sets a value into member of the specified property group. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetProperty<TControl, TProperty>(this TControl control, Func<TControl, VirtualPropertyGroupDictionary<TProperty>> prop, string key, TProperty value)
            where TControl : DotvvmBindableObject
        {
            var d = prop(control);
            d.Add(key, value);
            return control;
        }

        /// <summary> Sets a value or a binding into member of the specified property group. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetProperty<TControl, TProperty>(this TControl control, Func<TControl, VirtualPropertyGroupDictionary<TProperty>> prop, string key, ValueOrBinding<TProperty> value)
            where TControl : DotvvmBindableObject
        {
            var d = prop(control);
            d.Add(key, value);
            return control;
        }

        /// <summary> Sets a value (or a binding) into the specified html attribute. If the value is null, the attribute is removed. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetAttribute<TControl>(this TControl control, string attribute, object? value)
            where TControl : IControlWithHtmlAttributes
        {
            value = ValueOrBindingExtensions.UnwrapToObject(value);

            if (value is not null)
            {
                control.Attributes[attribute] = value;
            }
            else
            {
                control.Attributes.Remove(attribute);
            }
            return control;
        }

        /// <summary> Sets a value or a binding into the specified html attribute. If the value is null, the attribute is removed. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetAttribute<TControl, TValue>(this TControl control, string attribute, ValueOrBinding<TValue> value)
            where TControl : IControlWithHtmlAttributes
        {
            return SetAttribute(control, attribute, value.UnwrapToObject());
        }

        /// <summary> Sets a value or a binding into the specified html attribute. If the value is null, the attribute is removed. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetAttribute<TControl, TValue>(this TControl control, string attribute, ValueOrBinding<TValue>? value)
            where TControl : IControlWithHtmlAttributes
        {
            return SetAttribute(control, attribute, value?.UnwrapToObject());
        }

        /// <summary> Sets all properties from the capability into this control. If the control does not support the capability, exception is thrown. Returns <paramref name="control"/> for fluent API usage. </summary>
        public static TControl SetCapability<TControl, TCapability>(this TControl control, [AllowNull] TCapability capability, string prefix = "")
            where TControl: DotvvmBindableObject
        {
            if (capability != null)
            {
                var c = DotvvmCapabilityProperty.Find(typeof(TControl), typeof(TCapability), prefix);
                if (c is null)
                    throw new Exception($"Capability {prefix}{typeof(TCapability)} is not defined on {typeof(TControl)}");
                c.SetValue(control, capability);
            }
            return control;
        }

        /// <summary> Adds all <paramref name="children"/> into control.Children (nulls are skipped). Returns <paramref name="control"/> for fluent API usage. </summary>
        public static T AppendChildren<T>(this T control, params DotvvmControl?[]? children)
            where T : DotvvmControl
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child is not null)
                        control.Children.Add(child);
                }
            }

            return control;

        }

        /// <summary> Adds all <paramref name="children"/> into control.Children (nulls are skipped). Returns <paramref name="control"/> for fluent API usage. </summary>
        public static T AppendChildren<T>(this T control, IEnumerable<DotvvmControl?>? children)
            where T : DotvvmControl 
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child is not null)
                        control.Children.Add(child);
                }
            }

            return control;
        }

        /// <summary>
        /// Gets the value binding set to the DotvvmProperty referenced in the lambda expression. Returns null if the property is not a binding, throws if the binding some kind of command.
        /// </summary>
        public static IValueBinding<TProperty>? GetValueBinding<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
            => (IValueBinding<TProperty>?)control.GetValueBinding(control.GetDotvvmProperty(prop));

        /// <summary>
        /// Gets the command binding set to the DotvvmProperty referenced in the lambda expression. Returns null if the property is not a binding, throws if the binding is not command, controlCommand or staticCommand.
        /// </summary>
        public static ICommandBinding<TProperty>? GetCommandBinding<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
            => (ICommandBinding<TProperty>?)control.GetCommandBinding(control.GetDotvvmProperty(prop));

        /// <summary> Returns the value of the DotvvmProperty referenced in the lambda expression. If the property contains a binding, it is evaluted. </summary>
        [return: MaybeNull]
        public static TProperty GetValue<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
            => control.GetValue<TProperty>(control.GetDotvvmProperty(prop));

        /// <summary> Gets the value of the DotvvmProperty referenced in the lambda expression. Bindings are always returned, not evaluated. </summary>
        public static ValueOrBinding<TProperty> GetValueOrBinding<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
            => control.GetValueOrBinding<TProperty>(control.GetDotvvmProperty(prop));

        /// <summary> Gets the specified control capability - reads all the properties in the capability at once. Throws if this control does not support the capability. </summary>
        public static TCapability GetCapability<TCapability>(this DotvvmBindableObject control, string prefix = "")
        {
            var c = DotvvmCapabilityProperty.Find(control.GetType(), typeof(TCapability), prefix);
            if (c is null)
                throw new Exception($"Capability {prefix}{typeof(TCapability)} is not defined on {control.GetType()}");
            return (TCapability)c.GetValue(control)!;
        }

        internal static object? TryGetValue(this DotvvmBindableObject control, DotvvmProperty property)
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

        /// <summary> Returns somewhat readable string representing this dotvvm control. </summary>
        public static string DebugString(this DotvvmBindableObject control, DotvvmConfiguration? config = null, bool multiline = true)
        {
            if (control == null) return "null";

            config = config ?? (control.TryGetValue(Internal.RequestContextProperty) as IDotvvmRequestContext)?.Configuration;

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

            var location = (file: control.TryGetValue(Internal.MarkupFileNameProperty) as string, line: control.TryGetValue(Internal.MarkupLineNumberProperty) as int? ?? -1);
            var reg = config?.Markup.Controls.FirstOrDefault(c => c.Namespace == type.Namespace && Type.GetType(c.Namespace + "." + type.Name + ", " + c.Assembly) == type) ??
                      config?.Markup.Controls.FirstOrDefault(c => c.Namespace == type.Namespace) ??
                      config?.Markup.Controls.FirstOrDefault(c => c.Assembly == type.Assembly.GetName().Name);
            var ns = reg?.TagPrefix ?? (type.Namespace == "DotVVM.Framework.Controls" ? "dot" : "_");
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
