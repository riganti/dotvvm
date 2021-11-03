using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotNext.Linq.Expressions;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding
{
    public partial class DotvvmCapabilityProperty
    {
        internal static class CodeGeneration
        {
            public static (Expression<Func<DotvvmBindableObject, object>> getter, Expression<Action<DotvvmBindableObject, object?>> setter) CreateCapabilityAccessors(DotvvmCapabilityProperty capability)
            {
                var props = capability.PropertyMapping ?? throw new Exception("Capability property must have property mapping.");
                var pgroups = capability.PropertyGroupMapping!.Value;
                var capabilityType = capability.PropertyType;
                var valueParameter = Expression.Parameter(capabilityType, "value");
                var valueObjectParameter = Expression.Parameter(typeof(object), "valueOb");
                var getterBody = new List<Expression> {
                    valueParameter.Assign(capabilityType.New())
                };
                var setterBody = new List<Expression> {
                    valueParameter.Assign(valueObjectParameter.Convert(capabilityType))
                };

                foreach (var (prop, dotvvmProperty) in props)
                {
                    var (getter, setter) = CreatePropertyAccessors(prop.PropertyType, dotvvmProperty);
                    getterBody.Add(Expression.Assign(valueParameter.Property(prop), getter.Replace(currentControlParameter)));

                    setterBody.Add(setter.Replace(
                        currentControlParameter,
                        valueParameter.Property(prop)
                    ));
                }
                foreach (var (prop, propGroup) in pgroups)
                {
                    var (getter, setter) = CreatePropertyGroupAccessors(prop.PropertyType, propGroup);
                    getterBody.Add(Expression.Assign(valueParameter.Property(prop), getter.Replace(currentControlParameter)));

                    setterBody.Add(setter.Replace(
                        currentControlParameter,
                        valueParameter.Property(prop)
                    ));
                }

                getterBody.Add(valueParameter.Convert<object>());


                var capabilityGetter =
                    Expression.Lambda<Func<DotvvmBindableObject, object>>(
                        Expression.Block(new [] { valueParameter }, getterBody),
                        currentControlParameter);

                var capabilitySetter =
                    Expression.Lambda<Action<DotvvmBindableObject, object?>>(
                        Expression.Block(
                            new [] { valueParameter },
                            setterBody
                        ), currentControlParameter, valueObjectParameter);

                return (capabilityGetter, capabilitySetter);
            }

            public static (LambdaExpression getter, LambdaExpression setter) CreatePropertyGroupAccessors(Type type, DotvvmPropertyGroup pgroup)
            {
                var propType = pgroup.PropertyType;
                var elementType = Helpers.GetDictionaryElement(type);
                var valueParameter = Expression.Parameter(type, "value");
                var ctor = typeof(VirtualPropertyGroupDictionary<>)
                    .MakeGenericType(propType)
                    .GetConstructor(new [] { typeof(DotvvmBindableObject), typeof(DotvvmPropertyGroup) });
                var createMethod = typeof(VirtualPropertyGroupDictionary<>)
                    .MakeGenericType(propType)
                    .GetMethod(
                        typeof(ValueOrBinding).IsAssignableFrom(elementType) ? nameof(VirtualPropertyGroupDictionary<int>.CreatePropertyDictionary) :
                        nameof(VirtualPropertyGroupDictionary<int>.CreateValueDictionary),
                        BindingFlags.Public | BindingFlags.Static
                    );
                var enumerableType = typeof(IEnumerable<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), elementType));
                var copyFromMethod =
                    typeof(VirtualPropertyGroupDictionary<>)
                    .MakeGenericType(propType)
                    .GetMethod("CopyFrom", new [] { enumerableType, typeof(bool) });
                return (
                    Expression.Lambda(
                        Expression.Call(createMethod, currentControlParameter, Expression.Constant(pgroup))
                            .Convert(type),
                        currentControlParameter
                    ),
                    Expression.Lambda(
                        Expression.New(ctor, currentControlParameter, pgroup.Const()).Call(
                            copyFromMethod,
                            valueParameter.Convert(enumerableType),
                            true.Const() // clear
                        ),
                        currentControlParameter,
                        valueParameter
                    )
                );

            }
            public static (LambdaExpression getter, LambdaExpression setter) CreatePropertyAccessors(Type type, DotvvmProperty property)
            {
                var valueParameter = Expression.Parameter(type, "value");
                var unwrappedType = type.UnwrapNullableType();
                if (typeof(IBinding).IsAssignableFrom(type))
                {
                    var getValueRawMethod = typeof(DotvvmBindableObject).GetMethod("GetValueRaw");
                    var setValueRawMethod = typeof(DotvvmBindableObject).GetMethod("SetValueRaw", new[] { typeof(DotvvmProperty), typeof(object) });
                    return (
                        Expression.Lambda(
                            currentControlParameter.Call("GetValueRaw", property.Const(), false.Const())
                                .Convert(type),
                            currentControlParameter
                        ),
                        Expression.Lambda(
                            currentControlParameter.Call("SetValueRaw", property.Const(), valueParameter.Convert<object>()),
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
                            Expression.Call(
                                getValueOrBindingMethod,
                                currentControlParameter,
                                property.Const()),
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
