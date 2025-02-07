using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using static System.Linq.Expressions.Expression;

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
                    Assign(valueParameter, New(capabilityType))
                };
                var setterBody = new List<Expression> {
                    Assign(valueParameter, Convert(valueObjectParameter, capabilityType))
                };

                foreach (var (prop, dotvvmProperty) in props)
                {
                    var (getter, setter) = CreatePropertyAccessors(prop.PropertyType, dotvvmProperty);
                    getterBody.Add(Assign(Property(valueParameter, prop), getter.Replace(currentControlParameter)));

                    setterBody.Add(setter.Replace(
                        currentControlParameter,
                        Property(valueParameter, prop)
                    ));
                }
                foreach (var (prop, propGroup) in pgroups)
                {
                    var (getter, setter) = CreatePropertyGroupAccessors(prop.PropertyType, propGroup);
                    getterBody.Add(Expression.Assign(Property(valueParameter, prop), getter.Replace(currentControlParameter)));

                    setterBody.Add(setter.Replace(
                        currentControlParameter,
                        Property(valueParameter, prop)
                    ));
                }

                getterBody.Add(Convert(valueParameter, typeof(object)));


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
                    .GetConstructor(new [] { typeof(DotvvmBindableObject), typeof(DotvvmPropertyGroup) })!;
                var createMethod = typeof(VirtualPropertyGroupDictionary<>)
                    .MakeGenericType(propType)
                    .GetMethod(
                        typeof(ValueOrBinding).IsAssignableFrom(elementType) ? nameof(VirtualPropertyGroupDictionary<int>.CreatePropertyDictionary) :
                        nameof(VirtualPropertyGroupDictionary<int>.CreateValueDictionary),
                        BindingFlags.Public | BindingFlags.Static
                    )!;
                var enumerableType = typeof(IEnumerable<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), elementType));
                var copyFromMethod =
                    typeof(VirtualPropertyGroupDictionary<>)
                    .MakeGenericType(propType)
                    .GetMethod("CopyFrom", new [] { enumerableType, typeof(bool) })!;
                return (
                    Lambda(
                        Convert(Call(createMethod, currentControlParameter, Constant(pgroup)), type),
                        currentControlParameter
                    ),
                    Lambda(
                        Call(
                            New(ctor, currentControlParameter, Constant(pgroup)),
                            copyFromMethod,
                            Convert(valueParameter, enumerableType),
                            Constant(true) // clear
                        ),
                        currentControlParameter,
                        valueParameter
                    )
                );

            }
            public static (LambdaExpression getter, LambdaExpression setter) CreatePropertyAccessors(Type type, DotvvmProperty property)
            {
                if (property is DotvvmPropertyAlias propertyAlias)
                    return CreatePropertyAccessors(type, propertyAlias.Aliased);

                // if the property does not override GetValue/SetValue, we'll use
                // control.properties dictionary directly to avoid virtual method calls
                var canUseDirectAccess =
                    !property.IsValueInherited && (
                    property.GetType() == typeof(DotvvmProperty) ||
                    property.GetType().GetMethod(nameof(DotvvmProperty.GetValue), new [] { typeof(DotvvmBindableObject), typeof(bool) })!.DeclaringType == typeof(DotvvmProperty) &&
                    property.GetType().GetMethod(nameof(DotvvmProperty.SetValue), new [] { typeof(DotvvmBindableObject), typeof(object) })!.DeclaringType == typeof(DotvvmProperty));

                var valueParameter = Expression.Parameter(type, "value");
                var unwrappedType = type.UnwrapNullableType();

                var boxedValueParameter = TypeConversion.BoxToObject(valueParameter);
                var setValueRaw =
                    canUseDirectAccess
                        ? Call(typeof(Helpers), nameof(Helpers.SetValueDirect), Type.EmptyTypes, currentControlParameter, Constant(property), boxedValueParameter)
                        : Call(currentControlParameter, nameof(DotvvmBindableObject.SetValueRaw), Type.EmptyTypes, Constant(property), boxedValueParameter);

                if (typeof(IBinding).IsAssignableFrom(type))
                {
                    var getValueRaw =
                        canUseDirectAccess
                            ? Call(typeof(Helpers), nameof(Helpers.GetValueRawDirect), Type.EmptyTypes, currentControlParameter, Constant(property))
                            : Call(currentControlParameter, nameof(DotvvmBindableObject.GetValueRaw), Type.EmptyTypes, Constant(property), Constant(property.IsValueInherited));
                    return (
                        Lambda(
                            Convert(getValueRaw, type),
                            currentControlParameter
                        ),
                        Expression.Lambda(
                            setValueRaw,
                            currentControlParameter, valueParameter
                        )
                    );
                }
                else if (unwrappedType.IsGenericType && unwrappedType.GetGenericTypeDefinition() == typeof(ValueOrBinding<>))
                {
                    var isNullable = type.IsNullable();
                    var innerType = unwrappedType.GetGenericArguments().Single();
                    var getValueOrBindingMethod =
                        typeof(Helpers).GetMethod(
                            (isNullable, canUseDirectAccess) switch {
                                (true, true) => nameof(Helpers.GetOptionalValueOrBinding),
                                (true, false) => nameof(Helpers.GetOptionalValueOrBindingSlow),
                                (false, true) => nameof(Helpers.GetValueOrBinding),
                                (false, false) => nameof(Helpers.GetValueOrBindingSlow)
                            }
                        )!.MakeGenericMethod(innerType);
                    var setValueOrBindingMethod =
                        typeof(Helpers).GetMethod(
                            (isNullable, canUseDirectAccess) switch {
                                (true, true) => nameof(Helpers.SetOptionalValueOrBinding),
                                (true, false) => nameof(Helpers.SetOptionalValueOrBindingSlow),
                                (false, true) => nameof(Helpers.SetValueOrBinding),
                                (false, false) => nameof(Helpers.SetValueOrBindingSlow)
                            }
                        )!.MakeGenericMethod(innerType);
                    return (
                        Expression.Lambda(
                            Expression.Call(
                                getValueOrBindingMethod,
                                currentControlParameter,
                                Constant(property)),
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
                                          where m.Name == nameof(DotvvmBindableObject.GetValue) && !m.IsGenericMethod
                                          select m).Single();

                    Expression getValue;
                    if (canUseDirectAccess && unwrappedType.IsValueType)
                    {
                        getValue = Call(typeof(Helpers), nameof(Helpers.GetStructValueDirect), new Type[] { unwrappedType }, currentControlParameter, Constant(property));
                        if (!type.IsNullable())
                            getValue = Expression.Property(getValue, "Value");
                    }
                    else
                    {
                        getValue = Call(currentControlParameter, getValueMethod, Constant(property), Constant(property.IsValueInherited));
                        getValue = Convert(getValue, type);
                    }
                    return (
                        Expression.Lambda(
                            getValue,
                            currentControlParameter
                        ),
                        Expression.Lambda(
                            setValueRaw,
                            currentControlParameter, valueParameter
                        )
                    );
                }
            }

        }
    }
}
