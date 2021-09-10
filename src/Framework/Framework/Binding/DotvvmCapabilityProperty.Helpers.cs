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
                    var boxedVal = v.BindingOrDefault ?? v.BoxedValue;
                    c.properties.Set(p, boxedVal);
                }
                else
                {
                    c.properties.Remove(p);
                }
            }
            public static void SetValueOrBinding<T>(DotvvmBindableObject c, DotvvmProperty p, ValueOrBinding<T> val)
            {
                // TODO: remove the property in case of default value?
                var boxedVal = val.BindingOrDefault ?? val.BoxedValue;
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

            public static (LambdaExpression getter, LambdaExpression setter) CreatePropertyLambdas(Type type, ParameterExpression valueParameter, DotvvmProperty property)
            {
                var unwrappedType = type.UnwrapNullableType();
                if (typeof(IBinding).IsAssignableFrom(type))
                {
                    var getValueRawMethod = typeof(DotvvmBindableObject).GetMethod("GetValueRaw");
                    var setValueRawMethod = typeof(DotvvmBindableObject).GetMethod("SetValueRaw", new[] { typeof(DotvvmProperty), typeof(object) });
                    return (
                        Expression.Lambda(
                            Expression.Convert(
                                Expression.Call(currentControlParameter, getValueRawMethod, Expression.Constant(property), Expression.Constant(false)),
                                type
                            ),
                            currentControlParameter
                        ),
                        Expression.Lambda(
                            Expression.Call(currentControlParameter, setValueRawMethod, Expression.Constant(property), Expression.Convert(valueParameter, typeof(object))),
                            currentControlParameter, valueParameter
                        )
                    );
                }
                else if (unwrappedType.IsGenericType && unwrappedType.GetGenericTypeDefinition() == typeof(ValueOrBinding<>))
                {
                    // could hamper some optimizations, we can fix it later if needed
                    if (property.GetType() != typeof(DotvvmProperty))
                        throw new NotSupportedException($"Can not create getter/setter for ValueOrBinding and {property.GetType()}");
                    if (property.IsValueInherited)
                        throw new NotSupportedException($"Can not create getter/setter for ValueOrBinding and inherited property");

                    var isNullable = type.IsNullable();
                    var innerType = unwrappedType.GetGenericArguments().Single();
                    var getValueOrBindingMethod =
                        typeof(Helpers).GetMethod(
                            isNullable ? "GetOptionalValueOrBinding" : "GetValueOrBinding"
                        ).MakeGenericMethod(innerType);
                    var setValueOrBindingMethod =
                        typeof(Helpers).GetMethod(
                            isNullable ? "SetOptionalValueOrBinding" : "SetValueOrBinding"
                        ).MakeGenericMethod(innerType);
                    return (
                        Expression.Lambda(
                            Expression.Call(getValueOrBindingMethod, currentControlParameter, Expression.Constant(property)),
                            currentControlParameter
                        ),
                        Expression.Lambda(
                            Expression.Call(setValueOrBindingMethod, currentControlParameter, Expression.Constant(property), valueParameter),
                            currentControlParameter, valueParameter
                        )
                    );
                }
                else
                {
                    var getValueMethod = (from m in typeof(DotvvmBindableObject).GetMethods()
                                        where m.Name == "GetValue" && !m.IsGenericMethod
                                        select m).Single();
                    var setValueMethod = typeof(DotvvmBindableObject).GetMethod("SetValue", new[] { typeof(DotvvmProperty), typeof(object) });
                    return (
                        Expression.Lambda(
                            Expression.Convert(
                                Expression.Call(currentControlParameter, getValueMethod, Expression.Constant(property), Expression.Constant(false)),
                                type
                            ),
                            currentControlParameter
                        ),
                        Expression.Lambda(
                            Expression.Call(currentControlParameter, setValueMethod, Expression.Constant(property), Expression.Convert(valueParameter, typeof(object))),
                            currentControlParameter, valueParameter
                        )
                    );
                }
            }

        }
    }
}
