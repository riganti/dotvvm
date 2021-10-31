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

        private static void AssertPropertyNotDefined(Type declaringType, Type capabilityType, string propertyName, string globalPrefix, bool postContent = false)
        {
            var postContentHelp = postContent ? $"It seems that the capability {capabilityType} contains a property of the same type, which leads to the conflict. " : "";
            if (Find(declaringType, capabilityType, globalPrefix) != null)
                throw new($"Capability of type {capabilityType} is already registered on control {declaringType} with prefix '{globalPrefix}'. {postContentHelp}If you want to register it multiple times, consider giving it a different prefix.");
            var postContentHelp2 = postContent ? $"It seems that the capability contains a property of the same name, which leads to the conflict. " : "";
            if (DotvvmProperty.ResolveProperty(declaringType, propertyName) is DotvvmProperty existingProp)
                throw new($"Capability {propertyName} conflicts with existing property. {postContentHelp2}Consider giving the capability a different name.");
        }

        public static DotvvmCapabilityProperty RegisterCapability<TCapabilityType, TDeclaringType>(string globalPrefix = "", string? name = null, ICustomAttributeProvider? capabilityAttributeProvider = null) =>
            RegisterCapability(typeof(TDeclaringType), typeof(TCapabilityType), globalPrefix, name, capabilityAttributeProvider);
        public static DotvvmCapabilityProperty RegisterCapability(Type declaringType, Type capabilityType, string globalPrefix = "", string? name = null, ICustomAttributeProvider? capabilityAttributeProvider = null, DotvvmCapabilityProperty? declaringCapability = null)
        {
            name ??= globalPrefix + capabilityType.Name;

            AssertPropertyNotDefined(declaringType, capabilityType, name, globalPrefix, postContent: false);

            var dotnetFieldName = name.Replace("-", "_").Replace(":", "_");
            capabilityAttributeProvider ??=
                declaringType.GetProperty(dotnetFieldName) ??
                declaringType.GetField(dotnetFieldName) ??
                (ICustomAttributeProvider)declaringType.GetField(dotnetFieldName + "Property") ??
                throw new Exception($"Capability backing field could not be found and capabilityAttributeProvider argument was not provided. Property: {declaringType.Name}.{name}. Please declare a field or property named {dotnetFieldName}.");

            var prop = new DotvvmCapabilityProperty(globalPrefix) {
                Name = name,
                PropertyType = capabilityType,
                DeclaringType = declaringType,
                AttributeProvider = capabilityAttributeProvider!,
                OwningCapability = declaringCapability,
            };
            prop.AddUsedInCapability(declaringCapability);
            InitializeCapability(prop, declaringType, capabilityType, globalPrefix, capabilityAttributeProvider);

            AssertPropertyNotDefined(declaringType, capabilityType, name, globalPrefix, postContent: true);

            var valueParameter = Expression.Parameter(typeof(object), "value");

            return RegisterCapability(prop);
        }

        public static DotvvmCapabilityProperty RegisterCapability<TCapabilityType, TDeclaringType>(
            Func<TDeclaringType, TCapabilityType> getter,
            Action<TDeclaringType, TCapabilityType> setter,
            string prefix = "",
            string? name = null,
            ICustomAttributeProvider? capabilityAttributeProvider = null)
            where TCapabilityType : notnull
            where TDeclaringType : DotvvmBindableObject =>
            RegisterCapability(typeof(TDeclaringType), typeof(TCapabilityType), (o) => (object)getter((TDeclaringType)o), (o, x) => setter((TDeclaringType)o, (TCapabilityType)x!), prefix, name);
        public static DotvvmCapabilityProperty RegisterCapability(Type declaringType, Type capabilityType, Func<DotvvmBindableObject, object> getter, Action<DotvvmBindableObject, object?> setter, string prefix = "", string? name = null) =>
            RegisterCapability(
                new DotvvmCapabilityProperty(prefix) {
                    Getter = getter,
                    Setter = setter,
                    Name = name ?? prefix + capabilityType.Name,
                    DeclaringType = declaringType,
                    PropertyType = capabilityType
                }
            );

        static DotvvmCapabilityProperty RegisterCapability(DotvvmCapabilityProperty property)
        {
            var declaringType = property.DeclaringType.NotNull();
            var capabilityType = property.PropertyType.NotNull();
            var name = property.Name.NotNull();
            AssertPropertyNotDefined(declaringType, capabilityType, name, property.Prefix);
            var attributes = new CustomAttributesProvider(
                new MarkupOptionsAttribute
                {
                    MappingMode = MappingMode.Exclude
                }
            );
            DotvvmProperty.Register(name, capabilityType, declaringType, DBNull.Value, false, property, attributes);
            if (!capabilityRegistry.TryAdd((declaringType, capabilityType, property.Prefix), property))
                AssertPropertyNotDefined(declaringType, capabilityType, name, property.Prefix);
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
            var boxedDefaultValue = defaultValue?.UnwrapToObject();

            var valueParameter = Expression.Parameter(propertyType, "value");

            // Property Group
            if (attributeProvider.GetCustomAttribute<PropertyGroupAttribute>() is PropertyGroupAttribute groupAttribute)
            {
                var elementType = Helpers.GetDictionaryElement(propertyType);
                var unwrappedType = elementType.UnwrapValueOrBinding();

                var globalPrefix = declaringCapability?.Prefix ?? "";
                var propertyGroup = DotvvmPropertyGroup.Register(
                    declaringType,
                    groupAttribute.Prefixes.Select(p => globalPrefix + p).ToArray(),
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
                var prefix = attributeProvider.GetCustomAttribute<DotvvmControlCapabilityAttribute>()?.Prefix ?? "";

                DotvvmCapabilityProperty capability;
                if (Find(declaringType, propertyType, prefix) is {} existingProperty)
                {
                    checkPropertyConflict(existingProperty, propertyType);
                    capability = existingProperty;
                    capability.AddUsedInCapability(declaringCapability);
                }
                else
                {
                    capability = DotvvmCapabilityProperty.RegisterCapability(declaringType, propertyType, prefix, name: null, attributeProvider, declaringCapability);
                }
                return Helpers.CreatePropertyLambdas(propertyType, valueParameter, capability);
            }
            // Standard property
            else
            {
                var type = propertyType.UnwrapValueOrBinding().UnwrapNullableType();

                DotvvmProperty dotvvmProperty;
                if (DotvvmProperty.ResolveProperty(declaringType, propertyName) is {} existingProperty)
                {
                    checkPropertyConflict(existingProperty, type);
                    dotvvmProperty = existingProperty;
                }
                else
                {
                    dotvvmProperty = new DotvvmProperty(propertyName, type, declaringType, boxedDefaultValue, false, attributeProvider);
                    dotvvmProperty.OwningCapability = declaringCapability;

                    var isNullable = propertyType.IsNullable() || propertyType.UnwrapValueOrBinding().IsNullable();

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

                    DotvvmProperty.Register(dotvvmProperty);
                }

                dotvvmProperty.AddUsedInCapability(declaringCapability);

                return Helpers.CreatePropertyLambdas(propertyType, valueParameter, dotvvmProperty);
            }

            void checkPropertyConflict(DotvvmProperty existingProperty, Type newPropertyType)
            {
                CheckPropertyConflict(existingProperty, newPropertyType, declaringType, declaringCapability);
            }
        }
        static void CheckPropertyConflict(DotvvmProperty existingProperty, Type newPropertyType, Type declaringType, DotvvmCapabilityProperty? declaringCapability)
        {
            string error = "";
            // same type
            if (newPropertyType != existingProperty.PropertyType)
                error += $" The properties have different types: '{newPropertyType}' vs '{existingProperty.PropertyType}'.";

            // the existing property must be declared above this one
            if (existingProperty.OwningCapability is {} existingCapability && declaringCapability is object)
            {
                var commonAncestor = existingCapability.ThisAndOwners().Intersect(declaringCapability.ThisAndOwners()).FirstOrDefault();
                var commonAncestorStr = commonAncestor?.PropertyType.Name ?? declaringType.Name;
                if (!declaringCapability.IsOwnedByCapability(existingCapability))
                    error += $" The property is declared in capabilities {existingCapability.PropertyType.Name} and {declaringCapability.Name} - to resolve the conflict declare the property in {commonAncestorStr}.";
            }
            // It is allowed to share property when it's declared in the control (existingCapability is null)
            // And it's allowed to share property with GetContents parameter (declaringCapability is null)

            if (error.Length == 0)
                return;

            var capabilityType = declaringCapability?.PropertyType;
            var capabilityHelp = capabilityType is null ? "" : $"The property is being defined because it is in {capabilityType.Name}, you can set prefix of the capability to prevent conflict. ";
            var compositeHelp =
                capabilityType is null && typeof(CompositeControl).IsAssignableFrom(declaringType) ?
                $"The property is being defined because parameter of it's name is defined in the {declaringType}.GetContents method. " : "";
            throw new Exception($"Can not define property {declaringType}.{existingProperty.Name} as it already exists.{error} {capabilityHelp}");
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
