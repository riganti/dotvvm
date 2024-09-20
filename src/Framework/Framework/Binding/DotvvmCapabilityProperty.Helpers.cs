using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding
{
    public partial class DotvvmCapabilityProperty
    {
        internal static class Helpers
        {
            public static ValueOrBinding<T>? GetOptionalValueOrBinding<T>(DotvvmBindableObject c, DotvvmPropertyId p, object? defaultValue)
            {
                if (c.properties.TryGet(p, out var x))
                    return ValueOrBinding<T>.FromBoxedValue(x);
                else return null;
            }
            public static ValueOrBinding<T> GetValueOrBinding<T>(DotvvmBindableObject c, DotvvmPropertyId p, object? defaultValue)
            {
                if (!c.properties.TryGet(p, out var x))
                    x = defaultValue;
                return ValueOrBinding<T>.FromBoxedValue(x);
            }
            public static ValueOrBinding<T>? GetOptionalValueOrBindingSlow<T>(DotvvmBindableObject c, DotvvmProperty p, object? defaultValue)
            {
                if (c.IsPropertySet(p))
                    return ValueOrBinding<T>.FromBoxedValue(c.GetValue(p));
                else return null;
            }
            public static ValueOrBinding<T> GetValueOrBindingSlow<T>(DotvvmBindableObject c, DotvvmProperty p, object? defaultValue)
            {
                return ValueOrBinding<T>.FromBoxedValue(c.GetValue(p));
            }
            public static void SetOptionalValueOrBinding<T>(DotvvmBindableObject c, DotvvmPropertyId p, object? defaultValue, ValueOrBinding<T>? val)
            {
                if (val.HasValue)
                {
                    SetValueOrBinding<T>(c, p, defaultValue, val.GetValueOrDefault());
                }
                else
                {
                    c.properties.Remove(p);
                }
            }
            public static void SetValueOrBinding<T>(DotvvmBindableObject c, DotvvmPropertyId p, object? defaultValue, ValueOrBinding<T> val)
            {
                var boxedVal = val.UnwrapToObject();
                SetValueDirect(c, p, defaultValue, boxedVal);
            }
            public static void SetOptionalValueOrBindingSlow<T>(DotvvmBindableObject c, DotvvmProperty p, object? defaultValue, ValueOrBinding<T>? val)
            {
                if (val.HasValue)
                {
                    SetValueOrBindingSlow<T>(c, p, defaultValue, val.GetValueOrDefault());
                }
                else
                {
                    c.SetValue(p, defaultValue); // set to default value, just in case this property is backed in a different place than c.properties[p]
                    c.properties.Remove(p);
                }
            }
            public static void SetValueOrBindingSlow<T>(DotvvmBindableObject c, DotvvmProperty p, object? defaultValue, ValueOrBinding<T> val)
            {
                var boxedVal = val.UnwrapToObject();
                if (Object.Equals(boxedVal, defaultValue) && c.IsPropertySet(p))
                {
                    // setting to default value and the property is not set -> do nothing
                }
                else
                {
                    c.SetValue(p, boxedVal);
                }
            }

            public static object? GetValueRawDirect(DotvvmBindableObject c, DotvvmPropertyId p, object defaultValue)
            {
                if (c.properties.TryGet(p, out var x))
                {
                    return x;
                }
                else return defaultValue;
            }
            public static T? GetStructValueDirect<T>(DotvvmBindableObject c, DotvvmPropertyId p, T? defaultValue)
                where T: struct
            {
                if (c.properties.TryGet(p, out var x))
                {
                    if (x is null)
                        return null;
                    if (x is T xValue)
                        return xValue;
                    return (T?)c.EvalPropertyValue(p, x);
                }
                else return defaultValue;
            }
            public static void SetValueDirect(DotvvmBindableObject c, DotvvmPropertyId p, object? defaultValue, object? value)
            {
                if (Object.Equals(defaultValue, value) && !c.properties.Contains(p))
                {
                    // setting to default value and the property is not set -> do nothing
                }
                else
                {
                    c.properties.Set(p, value);
                }
            }

            public static Type GetDictionaryElement(Type type)
            {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) || type.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                {
                    var args = type.GetGenericArguments();
                    if (args[0] != typeof(string))
                        throw new Exception("Property group Dictionary must have a string key.");
                    else
                        return args[1];
                }
                else throw new NotSupportedException($"{type.FullName} is not supported property group type. Use IDictionary<K, V> or IReadOnlyDictionary<K, V>.");

            }

        }
    }
}
