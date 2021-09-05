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
    public partial class DotvvmCapabilityProperty : DotvvmProperty
    {
        internal Func<DotvvmBindableObject, object> Getter { get; private set; } = null!;
        internal Action<DotvvmBindableObject, object?> Setter { get; private set; } = null!;
        public string Prefix { get; }

        private static ConcurrentDictionary<(Type declaringType, Type capabilityType, string prefix), DotvvmCapabilityProperty> capabilityRegistry = new();

        private DotvvmCapabilityProperty(string prefix)
        {
            this.Prefix = prefix;
        }

        public override object GetValue(DotvvmBindableObject control, bool inherit = true) => Getter(control);

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

        public static IEnumerable<DotvvmCapabilityProperty> GetCapabilities(Type declaringType) =>
            capabilityRegistry.Values.Where(c => c.DeclaringType.IsAssignableFrom(declaringType));

        public IEnumerable<DotvvmCapabilityProperty> ThisAndOwners()
        {
            for (var x = this; x is object; x = x.OwningCapability)
                yield return x;
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

        public static DotvvmCapabilityProperty RegisterCapability(string name, Type declaringType, Type capabilityType, string globalPrefix = "", ICustomAttributeProvider? capabilityAttributeProvider = null, DotvvmCapabilityProperty? declaringCapability = null)
        {
            AssertNotDefined(declaringType, capabilityType, name, globalPrefix, postContent: false);

            var prop = new DotvvmCapabilityProperty(globalPrefix) {
                Name = name,
                PropertyType = capabilityType,
                DeclaringType = declaringType,
                AttributeProvider = capabilityAttributeProvider!,
            };
            if (declaringCapability is object)
            {
                prop.OwningCapability = declaringCapability;
                prop.UsedInCapabilities = prop.UsedInCapabilities.Add(declaringCapability);
            }
            InitializeCapability(prop, declaringType, capabilityType, globalPrefix, capabilityAttributeProvider);

            AssertNotDefined(declaringType, capabilityType, name, globalPrefix, postContent: true);

            var valueParameter = Expression.Parameter(typeof(object), "value");

            return RegisterCapability(prop);
        }

        public static DotvvmCapabilityProperty RegisterCapability(string name, Type declaringType, Type capabilityType, Func<DotvvmBindableObject, object> getter, Action<DotvvmBindableObject, object?> setter, string prefix = "") =>
            RegisterCapability(
                new DotvvmCapabilityProperty(prefix) {
                    Getter = getter,
                    Setter = setter,
                    Name = name,
                    DeclaringType = declaringType,
                    PropertyType = capabilityType
                }
            );

        static DotvvmCapabilityProperty RegisterCapability(DotvvmCapabilityProperty property)
        {
            var declaringType = property.DeclaringType.NotNull();
            var capabilityType = property.PropertyType.NotNull();
            var name = property.Name.NotNull();
            AssertNotDefined(declaringType, capabilityType, name, property.Prefix);
            var attributes = new CustomAttributesProvider(
                new MarkupOptionsAttribute
                {
                    MappingMode = MappingMode.Exclude
                }
            );
            DotvvmProperty.Register(name, capabilityType, declaringType, DBNull.Value, false, property, attributes);
            if (!capabilityRegistry.TryAdd((declaringType, capabilityType, property.Prefix), property))
                AssertNotDefined(declaringType, capabilityType, name, property.Prefix);
            return property;
        }

        static void InitializeCapability(DotvvmCapabilityProperty resultProperty, Type declaringType, Type capabilityType, string globalPrefix, ICustomAttributeProvider? parentAttributeProvider)
        {
            var properties = capabilityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            if (!capabilityType.IsDefined(typeof(DotvvmControlCapabilityAttribute)))
                throw new Exception($"Class {capabilityType} is used as a DotVVM capability, but it is missing the [DotvvmControlCapability] attribute.");

            if (properties.Length == 0)
                throw new Exception($"Capability {capabilityType} does not have any properties. It was registered as property in {declaringType}.");

            if (capabilityType.GetConstructor(Type.EmptyTypes) == null)
                throw new Exception($"Capability {capabilityType} does not have a parameterless constructor. It was registered as property in {declaringType}.");

            if (!capabilityType.IsSealed)
                throw new Exception($"Capability {capabilityType} must be a sealed class or a struct. It was registered as property in {declaringType}.");

            var instance = Activator.CreateInstance(capabilityType);
            var valueParameter = Expression.Parameter(capabilityType, "value");
            var valueObjectParameter = Expression.Parameter(typeof(object), "valueOb");
            var getterBody = new List<Expression> {
                Expression.Assign(valueParameter, Expression.New(capabilityType))
            };
            var setterBody = new List<Expression>();
            setterBody.Add(
                Expression.Assign(valueParameter, Expression.Convert(valueObjectParameter, capabilityType))
            );
            foreach (var prop in properties)
            {
                var defaultValue = ValueOrBinding<object>.FromBoxedValue(prop.GetValue(instance));
                var attrProvider = CombinedDataContextAttributeProvider.Create(parentAttributeProvider, prop);
                var (propGetter, propSetter) = InitializeArgument(attrProvider, globalPrefix + prop.Name, prop.PropertyType, declaringType, resultProperty, defaultValue);

                getterBody.Add(Expression.Assign(Expression.Property(valueParameter, prop), ExpressionUtils.Replace(propGetter, currentControlParameter)));

                setterBody.Add(ExpressionUtils.Replace(propSetter,
                    currentControlParameter,
                    Expression.Property(valueParameter, prop)
                ));
            }
            getterBody.Add(valueParameter);

            resultProperty.Getter =
                Expression.Lambda<Func<DotvvmBindableObject, object>>(
                    Expression.Convert(Expression.Block(new [] { valueParameter }, getterBody), typeof(object)),
                    currentControlParameter)
                .Compile();

            resultProperty.Setter =
                Expression.Lambda<Action<DotvvmBindableObject, object?>>(
                    Expression.Block(
                        new [] { valueParameter },
                        setterBody
                    ),
                    currentControlParameter, valueObjectParameter)
                .Compile();
        }

        private static readonly ParameterExpression currentControlParameter = Expression.Parameter(typeof(DotvvmBindableObject), "control");
        internal static (LambdaExpression getter, LambdaExpression setter) InitializeArgument(ICustomAttributeProvider attributeProvider, string propertyName, Type propertyType, Type declaringType, DotvvmCapabilityProperty? declaringCapability, ValueOrBinding<object>? defaultValue)
        {
            var capabilityType = declaringCapability?.PropertyType;
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

                if (declaringCapability is object)
                {
                    propertyGroup.OwningCapability = declaringCapability;
                    propertyGroup.UsedInCapabilities = propertyGroup.UsedInCapabilities.Add(declaringCapability);
                }

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
                var capability = DotvvmCapabilityProperty.RegisterCapability(propertyName, declaringType, propertyType, prefix, attributeProvider, declaringCapability);
                return CreatePropertyLambdas(propertyType, valueParameter, capability);
            }
            // Standard property
            else
            {
                checkNameConflict(propertyName);
                var type = propertyType.UnwrapNullableType();
                type =
                    type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueOrBinding<>) ?
                    type.GenericTypeArguments.Single() :
                    type;
                var isNullable = propertyType.IsNullable() || type.IsNullable();
                type = type.UnwrapNullableType();

                var dotvvmProperty = new DotvvmProperty();
                DotvvmProperty.Register(propertyName, type, declaringType, boxedDefaultValue, false, dotvvmProperty, attributeProvider);

                if (declaringCapability is object)
                {
                    dotvvmProperty.OwningCapability = declaringCapability;
                    dotvvmProperty.UsedInCapabilities = dotvvmProperty.UsedInCapabilities.Add(declaringCapability);
                }

                if (!defaultValue.HasValue && !isNullable)
                    dotvvmProperty.MarkupOptions.Required = true;

                if (typeof(IBinding).IsAssignableFrom(propertyType))
                    dotvvmProperty.MarkupOptions.AllowHardCodedValue = false;
                else if (!typeof(ValueOrBinding).IsAssignableFrom(propertyType.UnwrapNullableType()))
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
        }
    }

    public class DotvvmControlCapabilityAttribute : Attribute
    {
        public string Prefix { get; }
        public DotvvmControlCapabilityAttribute(string prefix = "")
        {
            this.Prefix = prefix;
        }
    }
}
