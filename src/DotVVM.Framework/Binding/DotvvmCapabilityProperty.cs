#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding
{
    public class DotvvmCapabilityProperty : DotvvmProperty
    {
        public Func<DotvvmBindableObject, object?> Getter { get; }
        public Action<DotvvmBindableObject, object?> Setter { get; }
        public string Prefix { get; }

        private static ConcurrentDictionary<(Type declaringType, Type capabilityType, string prefix), DotvvmCapabilityProperty> capabilityRegistry = new();

        private DotvvmCapabilityProperty(Func<DotvvmBindableObject, object?> getter, Action<DotvvmBindableObject, object?> setter, string prefix)
        {
            this.Getter = getter ?? throw new ArgumentNullException(nameof(getter));
            this.Setter = setter ?? throw new ArgumentNullException(nameof(setter));
            this.Prefix = prefix;
        }

        public override object? GetValue(DotvvmBindableObject control, bool inherit = true) => Getter(control);

        public override void SetValue(DotvvmBindableObject control, object? value) => Setter(control, value);

        public static DotvvmCapabilityProperty? Find(Type declaringType, Type capabilityType, string globalPrefix = "")
        {
            while (declaringType != typeof(DotvvmBindableObject) && declaringType is not null)
            {
                if (capabilityRegistry.TryGetValue((declaringType, capabilityType, globalPrefix), out var result))
                    return result;
                declaringType = declaringType.BaseType;
            }
            return null;
        }

        private static void AssertNotDefined(Type declaringType, Type capabilityType, string propertyName, string globalPrefix, bool postContent = false)
        {
            var postContentHelp = postContent ? $"It seems that the capability {capabilityType} contains a property of the same type, which leads to the conflict. " : "";
            if (Find(declaringType, capabilityType, globalPrefix) != null)
                throw new($"Capability of type {capabilityType} is already registered on control {declaringType} with prefix '{globalPrefix}'. {postContentHelp}If you want to register it multiple times, consider giving it a different prefix.");
            var postContentHelp2 = postContent ? $"It seems that the capability contains a property of the same name, which leads to the conflict. " : "";
            if (DotvvmProperty.ResolveProperty(declaringType, propertyName) is DotvvmProperty existingProp)
                throw new($"Capability {propertyName} conflicts with existing property. {postContentHelp2}Consider giving the capability a different name.");
        }

        public static DotvvmCapabilityProperty RegisterCapability(string name, Type declaringType, Type capabilityType, string globalPrefix = "")
        {
            AssertNotDefined(declaringType, capabilityType, name, globalPrefix, postContent: true);
            var (getterExpression, setterExpression) = InitializeCapability(declaringType, capabilityType, globalPrefix);
            AssertNotDefined(declaringType, capabilityType, name, globalPrefix, postContent: true);

            var getter = Expression.Lambda<Func<DotvvmBindableObject, object>>(
                            Expression.Convert(getterExpression.Body, typeof(object)),
                            getterExpression.Parameters)
                         .Compile();
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var setter = Expression.Lambda<Action<DotvvmBindableObject, object?>>(
                            ExpressionUtils.Replace(setterExpression, currentControlParameter, Expression.Convert(valueParameter, capabilityType)),
                            currentControlParameter, valueParameter)
                         .Compile();

            return RegisterCapability(name, declaringType, capabilityType, getter, setter, globalPrefix);
        }

        public static DotvvmCapabilityProperty RegisterCapability(string name, Type declaringType, Type capabilityType, Func<DotvvmBindableObject, object?> getter, Action<DotvvmBindableObject, object?> setter, string prefix = "")
        {
            AssertNotDefined(declaringType, capabilityType, name, prefix);
            var property = new DotvvmCapabilityProperty(getter, setter, prefix);
            var attributes = new CustomAttributesProvider(
                new MarkupOptionsAttribute
                {
                    MappingMode = MappingMode.Exclude
                }
            );
            DotvvmProperty.Register(name, capabilityType, declaringType, DBNull.Value, false, property, attributes);
            if (!capabilityRegistry.TryAdd((declaringType, capabilityType, prefix), property))
                AssertNotDefined(declaringType, capabilityType, name, prefix);
            return property;
        }

        class CustomAttributesProvider : ICustomAttributeProvider
        {
            private readonly object[] attributes;
            public CustomAttributesProvider(params object[] attributes)
            {
                this.attributes = attributes;
            }
            public object[] GetCustomAttributes(bool inherit) => attributes;

            public object[] GetCustomAttributes(Type attributeType, bool inherit) => GetCustomAttributes(inherit).Where(attributeType.IsInstanceOfType).ToArray();

            public bool IsDefined(Type attributeType, bool inherit) => GetCustomAttributes(attributeType, inherit).Any();
        }

        static (LambdaExpression getter, LambdaExpression setter) InitializeCapability(Type declaringType, Type capabilityType, string globalPrefix = "")
        {
            var properties = capabilityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var instance = Activator.CreateInstance(capabilityType);
            var valueParameter = Expression.Parameter(capabilityType, "value");
            var getterBody = new List<Expression> {
                Expression.Assign(valueParameter, Expression.New(capabilityType))
            };
            var setterBody = new List<Expression>();
            foreach (var prop in properties)
            {
                var defaultValue = ValueOrBinding<object>.FromBoxedValue(prop.GetValue(instance));
                var (propGetter, propSetter) = InitializeArgument(prop, globalPrefix + prop.Name, prop.PropertyType, declaringType, capabilityType, defaultValue);

                getterBody.Add(Expression.Assign(Expression.Property(valueParameter, prop), ExpressionUtils.Replace(propGetter, currentControlParameter)));

                setterBody.Add(ExpressionUtils.Replace(propSetter,
                    currentControlParameter,
                    Expression.Property(valueParameter, prop)
                ));
            }
            getterBody.Add(valueParameter);

            return (
                Expression.Lambda(
                    Expression.Block(new [] { valueParameter }, getterBody),
                    currentControlParameter
                ),
                Expression.Lambda(
                    Expression.Block(setterBody),
                    currentControlParameter,
                    valueParameter
                )
            );
        }

        private static readonly ParameterExpression currentControlParameter = Expression.Parameter(typeof(DotvvmBindableObject), "control");
        internal static (LambdaExpression getter, LambdaExpression setter) InitializeArgument(ICustomAttributeProvider attributeProvider, string propertyName, Type propertyType, Type declaringType, Type? capabilityType, ValueOrBinding<object>? defaultValue = null)
        {
            propertyName = char.ToUpperInvariant(propertyName[0]) + propertyName.Substring(1);

            if (attributeProvider.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultAttribute)
            {
                defaultValue = ValueOrBinding<object>.FromBoxedValue(defaultAttribute.Value);
            }
            var boxedDefaultValue = defaultValue?.BindingOrDefault ?? defaultValue?.BoxedValue;

            var valueParameter = Expression.Parameter(propertyType, "value");

            // Property Group
            if (attributeProvider.GetCustomAttribute<PropertyGroupAttribute>() is PropertyGroupAttribute groupAttribute)
            {
                // get value type from dictionary
                var elementType =
                    propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>) || propertyType.GetGenericTypeDefinition() == typeof(IDictionary<,>)) ?
                        propertyType.GetGenericArguments()
                        .Assert(p => p[0] == typeof(string))
                        [1] :
                    throw new NotSupportedException($"{propertyType.FullName} is not supported property group type. Use IDictionary<K, V> or IReadOnlyDictionary<K, V>.");

                var unwrappedType =
                    elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(ValueOrBinding<>) ?
                    elementType.GenericTypeArguments.Single() :
                    elementType;


                var propertyGroup = DotvvmPropertyGroup.Register(
                    declaringType,
                    groupAttribute.Prefixes,
                    propertyName,
                    unwrappedType,
                    attributeProvider,
                    boxedDefaultValue
                );

                var ctor = typeof(VirtualPropertyGroupDictionary<>)
                    .MakeGenericType(unwrappedType)
                    .GetConstructor(new [] { typeof(DotvvmBindableObject), typeof(DotvvmPropertyGroup) });
                var createMethod = typeof(VirtualPropertyGroupDictionary<>)
                    .MakeGenericType(unwrappedType)
                    .GetMethod(
                        typeof(ValueOrBinding).IsAssignableFrom(elementType) ? nameof(VirtualPropertyGroupDictionary<int>.CreatePropertyDictionary) :
                        nameof(VirtualPropertyGroupDictionary<int>.CreateValueDictionary),
                        BindingFlags.Public | BindingFlags.Static
                    );
                var enumerableType = typeof(IEnumerable<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(typeof(string), elementType));
                var copyFromMethod =
                    typeof(VirtualPropertyGroupDictionary<>)
                    .MakeGenericType(unwrappedType)
                    .GetMethod("CopyFrom", new [] { enumerableType, typeof(bool) });
                return (
                    Expression.Lambda(
                        Expression.Convert(
                            Expression.Call(createMethod, currentControlParameter, Expression.Constant(propertyGroup)),
                            propertyType
                        ),
                        currentControlParameter
                    ),
                    Expression.Lambda(
                        Expression.Call(
                            Expression.New(ctor, currentControlParameter, Expression.Constant(propertyGroup)),
                            copyFromMethod,
                            Expression.Convert(valueParameter, enumerableType),
                            Expression.Constant(true) // clear
                        ),
                        currentControlParameter,
                        valueParameter
                    )
                );
            }
            // Control Capability
            else if (propertyType.IsDefined(typeof(DotvvmControlCapabilityAttribute)) || attributeProvider.IsDefined(typeof(DotvvmControlCapabilityAttribute), true))
            {
                // auto append Capability to the end. Tends to prevent conflicts
                if (!propertyName.EndsWith("capability", StringComparison.OrdinalIgnoreCase))
                    propertyName += "Capability";
                checkNameConflict(propertyName);

                var prefix = attributeProvider.GetCustomAttribute<DotvvmControlCapabilityAttribute>()?.Prefix ?? "";
                var capability = DotvvmCapabilityProperty.RegisterCapability(propertyName, declaringType, propertyType, prefix);
                return CreatePropertyLambdas(propertyType, valueParameter, capability);
            }
            // Standard property
            else
            {
                checkNameConflict(propertyName);

                var dotvvmProperty = new DotvvmProperty();
                DotvvmProperty.Register(propertyName, type, declaringType, boxedDefaultValue, false, dotvvmProperty, attributeProvider);

                if (!defaultValue.HasValue)
                    dotvvmProperty.MarkupOptions.Required = true;

                if (typeof(IBinding).IsAssignableFrom(propertyType))
                    dotvvmProperty.MarkupOptions.AllowHardCodedValue = false;
                else if (!typeof(ValueOrBinding).IsAssignableFrom(propertyType))
                    dotvvmProperty.MarkupOptions.AllowBinding = false;

                if (typeof(DotvvmBindableObject).IsAssignableFrom(type) ||
                    typeof(ITemplate).IsAssignableFrom(type) ||
                    typeof(IEnumerable<DotvvmBindableObject>).IsAssignableFrom(type))
                    dotvvmProperty.MarkupOptions.MappingMode = MappingMode.Both;

                return CreatePropertyLambdas(propertyType, valueParameter, dotvvmProperty);
            }

            void checkNameConflict(string propertyName)
            {
                if (DotvvmProperty.ResolveProperty(declaringType, propertyName) is DotvvmProperty existingProperty)
                {
                    var capabilityHelp = capabilityType is null ? "" : $"The property is being defined because it is in {capabilityType} capability, you can set prefix of the capability to prevent conflict. ";
                    var compositeHelp =
                        capabilityType is null && typeof(CompositeControl).IsAssignableFrom(declaringType) ?
                        $"The property is being defined because parameter of it's name is defined in the {declaringType}.GetContents method. " : "";
                    throw new Exception($"Can not define property {declaringType}.{propertyName} as it already exists. {capabilityHelp}");
                }
            }
        }

        private static (LambdaExpression getter, LambdaExpression setter) CreatePropertyLambdas(Type type, ParameterExpression valueParameter, DotvvmProperty property)
        {
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
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueOrBinding<>))
            {
                var getValueOrBindingMethod = typeof(DotvvmBindableObject).GetMethod("GetValueOrBinding").MakeGenericMethod(property.PropertyType);
                var setValueOrBindingMethod = typeof(DotvvmBindableObject).GetMethods().Single(m => m.Name == "SetValue" && m.IsGenericMethodDefinition).MakeGenericMethod(property.PropertyType);
                return (
                    Expression.Lambda(
                        Expression.Call(currentControlParameter, getValueOrBindingMethod, Expression.Constant(property), Expression.Constant(false)),
                        currentControlParameter
                    ),
                    Expression.Lambda(
                        Expression.Call(currentControlParameter, setValueOrBindingMethod, Expression.Constant(property), valueParameter),
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

    public class DotvvmControlCapabilityAttribute : Attribute
    {
        public string Prefix { get; }
        public bool Optional { get; }
        public DotvvmControlCapabilityAttribute(string prefix = "", bool optional = false)
        {
            if (optional) throw new NotSupportedException("Optional capabilities are not supported.");
            this.Optional = optional;
            this.Prefix = prefix;
        }
    }
}
