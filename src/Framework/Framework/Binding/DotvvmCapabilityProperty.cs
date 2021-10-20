using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding
{
    /// <summary> Descriptor of a DotVVM capability.
    /// Capability is a way to register multiple properties at once in DotVVM. </summary>
    public partial class DotvvmCapabilityProperty : DotvvmProperty
    {
        internal Func<DotvvmBindableObject, object> Getter { get; private set; } = null!;
        internal Action<DotvvmBindableObject, object?> Setter { get; private set; } = null!;
        /// <summary> List of properties that this capability contains. Note that this may contain nested capabilities. </summary>
        public ImmutableArray<(PropertyInfo prop, DotvvmProperty dotvvmProperty)>? PropertyMapping { get; private set; }
        /// <summary> List of property groups that this capability contains. Note that other property groups may be in nested capabilities (see the <see cref="PropertyMapping" /> array). </summary>
        public ImmutableArray<(PropertyInfo prop, DotvvmPropertyGroup dotvvmPropertyGroup)>? PropertyGroupMapping { get; private set; }
        /// <summary> Prefix prepended to all properties registered by this capability. </summary>
        public string Prefix { get; }

        private static ConcurrentDictionary<(Type declaringType, Type capabilityType, string prefix), DotvvmCapabilityProperty> capabilityRegistry = new();

        private DotvvmCapabilityProperty(
            string prefix,
            string? name,
            Type type,
            Type declaringType,
            ICustomAttributeProvider? attributeProvider
        ): base()
        {
            name ??= prefix + type.Name;
            AssertPropertyNotDefined(declaringType, type, name, prefix, postContent: false);

            if (!type.IsDefined(typeof(DotvvmControlCapabilityAttribute)))
                throw new Exception($"Class {type} is used as a DotVVM capability, but it is missing the [DotvvmControlCapability] attribute.");

            if (!type.IsSealed)
                throw new Exception($"Capability {type} should be a sealed record with {{ init; get; }} properties (also may be a sealed class or a struct). It was registered as property in {declaringType}.");

            this.Name = name;
            this.PropertyType = type;
            this.DeclaringType = declaringType;
            this.Prefix = prefix;

            var dotnetFieldName = name.Replace("-", "_").Replace(":", "_");
            attributeProvider ??=
                declaringType.GetProperty(dotnetFieldName) ??
                declaringType.GetField(dotnetFieldName) ??
                (ICustomAttributeProvider)declaringType.GetField(dotnetFieldName + "Property") ??
                throw new Exception($"Capability backing field could not be found and capabilityAttributeProvider argument was not provided. Property: {declaringType.Name}.{name}. Please declare a field or property named {dotnetFieldName}.");

            DotvvmProperty.InitializeProperty(this, attributeProvider);
        }

        public override object GetValue(DotvvmBindableObject control, bool inherit = true) => Getter(control);

        public override void SetValue(DotvvmBindableObject control, object? value) => Setter(control, value);

        /// <summary> Looks up a capability on the specified control (<paramref name="declaringType"/>). </summary>
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

        /// <summary> Lists capabilities on the specified control (<paramref name="declaringType"/>). </summary>
        public static IEnumerable<DotvvmCapabilityProperty> GetCapabilities(Type declaringType) =>
            capabilityRegistry.Values.Where(c => c.DeclaringType.IsAssignableFrom(declaringType));

        /// <summary> Returns an iterator of the <see cref="DotvvmProperty.OwningCapability" /> chain. The first element is this capability. </summary>
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

        /// <summary> Registers a new DotVVM capability. For a given <typeparamref name="TCapabilityType"/>, this method will register a DotVVM property for each property of the capability type. </summary>
        /// <param name="globalPrefix"> Prefix prepended to all properties registered by this capability. </param>
        /// <param name="name"> Name of the DotvvmProperty which will contain the capability. If not specified, name of <typeparamref name="TCapabilityType"/> will be used. </param>
        /// <param name="capabilityAttributeProvider"> A member info from System.Reflection which will be used to look for attributes. If not specified, DotVVM will look for property or field with the specified <paramref name="name"/>. </param>
        public static DotvvmCapabilityProperty RegisterCapability<TCapabilityType, TDeclaringType>(string globalPrefix = "", string? name = null, ICustomAttributeProvider? capabilityAttributeProvider = null) =>
            RegisterCapability(typeof(TDeclaringType), typeof(TCapabilityType), globalPrefix, name, capabilityAttributeProvider);
        /// <summary> Registers a new DotVVM capability. For a given <paramref name="capabilityType"/>, this method will register a DotVVM property for each property of the capability type. </summary>
        /// <param name="globalPrefix"> Prefix prepended to all properties registered by this capability. </param>
        /// <param name="name"> Name of the DotvvmProperty which will contain the capability. If not specified, name of <paramref name="capabilityType"/> will be used. </param>
        /// <param name="capabilityAttributeProvider"> A member info from System.Reflection which will be used to look for attributes. If not specified, DotVVM will look for property or field with the specified <paramref name="name"/>. </param>
        public static DotvvmCapabilityProperty RegisterCapability(Type declaringType, Type capabilityType, string globalPrefix = "", string? name = null, ICustomAttributeProvider? capabilityAttributeProvider = null, DotvvmCapabilityProperty? declaringCapability = null)
        {
            var prop = new DotvvmCapabilityProperty(
                globalPrefix,
                name,
                capabilityType,
                declaringType,
                capabilityAttributeProvider!
            ) { 
                OwningCapability = declaringCapability
            };
            prop.AddUsedInCapability(declaringCapability);
            InitializeCapability(prop, declaringType, capabilityType, globalPrefix);

            AssertPropertyNotDefined(declaringType, capabilityType, prop.Name, globalPrefix, postContent: true);

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
                new DotvvmCapabilityProperty(prefix, name, capabilityType, declaringType, null) {
                    Getter = getter,
                    Setter = setter,
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

        static void InitializeCapability(DotvvmCapabilityProperty resultProperty, Type declaringType, Type capabilityType, string globalPrefix)
        {
            var properties = capabilityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            if (properties.Length == 0)
                throw new Exception($"Capability {capabilityType} does not have any properties. It was registered as property in {declaringType}.");

            if (capabilityType.GetConstructor(Type.EmptyTypes) == null)
                throw new Exception($"Capability {capabilityType} does not have a parameterless constructor. It was registered as property in {declaringType}.");

            if (resultProperty.PropertyMapping == null)
            {
                Debug.Assert(resultProperty.PropertyGroupMapping == null);
                var instance = Activator.CreateInstance(capabilityType);

                var definedProps = new List<(PropertyInfo, DotvvmProperty)>();
                var definedPGroups = new List<(PropertyInfo, DotvvmPropertyGroup)>();

                foreach (var prop in properties)
                {
                    var defaultValue = ValueOrBinding<object>.FromBoxedValue(prop.GetValue(instance));
                    var attrProvider = CombinedDataContextAttributeProvider.Create(resultProperty.AttributeProvider, prop);
                    var newProperty = InitializeArgument(attrProvider, globalPrefix + prop.Name, prop.PropertyType, declaringType, resultProperty, defaultValue);

                    if (newProperty is DotvvmProperty p)
                        definedProps.Add((prop, p));
                    else if (newProperty is DotvvmPropertyGroup g)
                        definedPGroups.Add((prop, g));
                }

                resultProperty.PropertyMapping = definedProps.ToImmutableArray();
                resultProperty.PropertyGroupMapping = definedPGroups.ToImmutableArray();
            }

            var accessors = CodeGeneration.CreateCapabilityAccessors(resultProperty);
            resultProperty.Getter = accessors.getter.Compile();
            resultProperty.Setter = accessors.setter.Compile();
        }

        private static readonly ParameterExpression currentControlParameter = Expression.Parameter(typeof(DotvvmBindableObject), "control");
        /// <summary> Returns DotvvmProperty, DotvvmCapabilityProperty or DotvvmPRopertyGroup </summary>
        internal static object InitializeArgument(ICustomAttributeProvider attributeProvider, string propertyName, Type propertyType, Type declaringType, DotvvmCapabilityProperty? declaringCapability, ValueOrBinding<object>? defaultValue)
        {
            var capabilityType = declaringCapability?.PropertyType;
            propertyName = char.ToUpperInvariant(propertyName[0]) + propertyName.Substring(1);

            if (attributeProvider.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultAttribute)
            {
                defaultValue = ValueOrBinding<object>.FromBoxedValue(defaultAttribute.Value);
            }
            var boxedDefaultValue = defaultValue?.UnwrapToObject();

            // Property Group
            if (attributeProvider.GetCustomAttribute<PropertyGroupAttribute>() is PropertyGroupAttribute groupAttribute)
            {
                var elementType = Helpers.GetDictionaryElement(propertyType);
                var unwrappedType = elementType.UnwrapValueOrBinding();

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
                return propertyGroup;
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
                return capability;
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

                return dotvvmProperty;
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

    /// <summary> This attribute is used for marking a DotVVM capability type. It can be also used to mark a capability property inside another capability. </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class DotvvmControlCapabilityAttribute : Attribute
    {
        public string Prefix { get; }
        public DotvvmControlCapabilityAttribute(string prefix = "")
        {
            this.Prefix = prefix;
        }
    }
}
