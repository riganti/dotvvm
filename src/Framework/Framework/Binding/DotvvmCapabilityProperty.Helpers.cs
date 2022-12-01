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
            public static ValueOrBinding<T>? GetOptionalValueOrBinding<T>(DotvvmBindableObject c, DotvvmProperty p)
            {
                if (c.properties.TryGet(p, out var x))
                    return ValueOrBinding<T>.FromBoxedValue(x);
                else return null;
            }
            public static ValueOrBinding<T> GetValueOrBinding<T>(DotvvmBindableObject c, DotvvmProperty p)
            {
                if (!c.properties.TryGet(p, out var x))
                    x = p.DefaultValue;
                return ValueOrBinding<T>.FromBoxedValue(x);
            }
            public static void SetOptionalValueOrBinding<T>(DotvvmBindableObject c, DotvvmProperty p, ValueOrBinding<T>? val)
            {
                if (val.HasValue)
                {
                    var v = val.GetValueOrDefault();
                    c.properties.Set(p, v.UnwrapToObject());
                }
                else
                {
                    c.properties.Remove(p);
                }
            }
            public static void SetValueOrBinding<T>(DotvvmBindableObject c, DotvvmProperty p, ValueOrBinding<T> val)
            {
                // TODO: remove the property in case of default value?
                var boxedVal = val.UnwrapToObject();
                c.properties.Set(p, boxedVal);
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
