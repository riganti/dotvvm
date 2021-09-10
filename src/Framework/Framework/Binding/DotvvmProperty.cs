using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using System.Diagnostics;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;

namespace DotVVM.Framework.Binding
{
    /// <summary>
    /// Represents a property of DotVVM controls.
    /// </summary>
    [DebuggerDisplay("{FullName}")]
    public class DotvvmProperty : IPropertyDescriptor
    {

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; protected set; }

        [JsonIgnore]
        ITypeDescriptor IControlAttributeDescriptor.DeclaringType => new ResolvedTypeDescriptor(DeclaringType);

        [JsonIgnore]
        ITypeDescriptor IControlAttributeDescriptor.PropertyType => new ResolvedTypeDescriptor(PropertyType);

        /// <summary>
        /// Gets the default value of the property.
        /// </summary>
        public object? DefaultValue { get; protected set; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        public Type PropertyType { get; protected set; }

        /// <summary>
        /// Gets the type of the class where the property is registered.
        /// </summary>
        public Type DeclaringType { get; protected set; }

        /// <summary>
        /// Gets whether the value can be inherited from the parent controls.
        /// </summary>
        public bool IsValueInherited { get; protected set; }

        /// <summary>
        /// Gets or sets the Reflection property information.
        /// </summary>
        [JsonIgnore]
        public PropertyInfo? PropertyInfo { get; private set; }

        /// <summary>
        /// Provider of custom attributes for this property.
        /// </summary>
        internal ICustomAttributeProvider AttributeProvider { get; set; }

        /// <summary>
        /// Gets or sets the markup options.
        /// </summary>
        public MarkupOptionsAttribute MarkupOptions { get; set; }

        /// <summary>
        /// Determines if property type inherits from IBinding
        /// </summary>
        public bool IsBindingProperty { get; private set; }

        /// <summary>
        /// Gets the full name of the descriptor.
        /// </summary>
        public string DescriptorFullName
        {
            get { return DeclaringType.FullName + "." + Name + "Property"; }
        }

        /// <summary>
        /// Gets the full name of the property.
        /// </summary>
        public string FullName
        {
            get { return DeclaringType.Name + "." + Name; }
        }

        [JsonIgnore]
        public DataContextChangeAttribute[] DataContextChangeAttributes { get; private set; }

        [JsonIgnore]
        public DataContextStackManipulationAttribute? DataContextManipulationAttribute { get; private set; }

        /// <summary> The capability which declared this property. When the property is declared by an capability, it can only be used by this capability. </summary>
        public DotvvmCapabilityProperty? OwningCapability { get; internal set; }
        /// <summary> The capabilities which use this property. </summary>
        public ImmutableArray<DotvvmCapabilityProperty> UsedInCapabilities { get; internal set; } = ImmutableArray<DotvvmCapabilityProperty>.Empty;


        internal void AddUsedInCapability(DotvvmCapabilityProperty? p)
        {
            if (p is object)
                lock(this)
                {
                    UsedInCapabilities = UsedInCapabilities.Add(p);
                }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DotvvmProperty"/> class from being created.
        /// </summary>
#pragma warning disable CS8618 // DotvvmProperty is usually initialized by InitializeProperty
        internal DotvvmProperty()
        {
        }
        internal DotvvmProperty(
#pragma warning restore CS8618
            string name,
            Type type,
            Type declaringType,
            object? defaultValue,
            bool isValueInherited,
            ICustomAttributeProvider attributeProvider
        )
        {
            this.Name = name;
            this.PropertyType = type;
            this.DeclaringType = declaringType;
            this.DefaultValue = defaultValue;
            this.IsValueInherited = isValueInherited;
            this.AttributeProvider = attributeProvider;
            InitializeProperty(this);
        }

        public IEnumerable<T> GetAttributes<T>() =>
            AttributeProvider.GetCustomAttributes<T>().Concat(
                PropertyInfo?.GetCustomAttributes<T>() ?? Enumerable.Empty<T>()
            );

        public bool IsOwnedByCapability(Type capability) =>
            (this is DotvvmCapabilityProperty && this.PropertyType == capability) ||
            OwningCapability?.IsOwnedByCapability(capability) == true;

        public bool IsOwnedByCapability(DotvvmCapabilityProperty capability) =>
            this == capability ||
            OwningCapability?.IsOwnedByCapability(capability) == true;

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public virtual object? GetValue(DotvvmBindableObject control, bool inherit = true)
        {
            if (control.properties.TryGet(this, out var value))
            {
                return value;
            }
            if (IsValueInherited && inherit && control.Parent != null)
            {
                return GetValue(control.Parent);
            }
            return DefaultValue;
        }


        /// <summary>
        /// Gets whether the value of the property is set
        /// </summary>
        public virtual bool IsSet(DotvvmBindableObject control, bool inherit = true)
        {
            if (control.properties.Contains(this))
            {
                return true;
            }

            if (IsValueInherited && inherit && control.Parent != null)
            {
                return IsSet(control.Parent);
            }

            return false;
        }


        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        public virtual void SetValue(DotvvmBindableObject control, object? value)
        {
            control.properties.Set(this, value);
        }

        /// <summary>
        /// Registers the specified DotVVM property.
        /// </summary>
        public static DotvvmProperty Register<TPropertyType, TDeclaringType>(Expression<Func<DotvvmProperty?>> fieldAccessor, [AllowNull] TPropertyType defaultValue = default(TPropertyType), bool isValueInherited = false)
        {
            var field = ReflectionUtils.GetMemberFromExpression(fieldAccessor) as FieldInfo;
            if (field == null || !field.IsStatic) throw new ArgumentException("The expression should be simple static field access", nameof(fieldAccessor));
            if (!field.Name.EndsWith("Property", StringComparison.Ordinal)) throw new ArgumentException($"DotVVM property backing field's '{field.Name}' name should end with 'Property'");
            return Register<TPropertyType, TDeclaringType>(field.Name.Remove(field.Name.Length - "Property".Length), defaultValue, isValueInherited);
        }

        /// <summary>
        /// Registers the specified DotVVM property.
        /// </summary>
        public static DotvvmProperty Register<TPropertyType, TDeclaringType>(Expression<Func<TDeclaringType, object?>> propertyAccessor, [AllowNull] TPropertyType defaultValue = default(TPropertyType), bool isValueInherited = false)
        {
            var property = ReflectionUtils.GetMemberFromExpression(propertyAccessor) as PropertyInfo;
            if (property == null) throw new ArgumentException("The expression should be simple property access", nameof(propertyAccessor));
            return Register<TPropertyType, TDeclaringType>(property.Name, defaultValue, isValueInherited);
        }

        /// <summary>
        /// Registers the specified DotVVM property.
        /// </summary>
        public static DotvvmProperty Register<TPropertyType, TDeclaringType>(string propertyName, [AllowNull] TPropertyType defaultValue = default(TPropertyType), bool isValueInherited = false, DotvvmProperty? property = null)
        {
            var field = typeof(TDeclaringType).GetField(propertyName + "Property", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) throw new ArgumentException($"'{typeof(TDeclaringType).Name}' does not contain static field '{propertyName}Property'.");

            return Register(propertyName, typeof(TPropertyType), typeof(TDeclaringType), defaultValue, isValueInherited, property, field);
        }

        public static DotvvmProperty Register(string propertyName, Type propertyType, Type declaringType, object? defaultValue, bool isValueInherited, DotvvmProperty? property, ICustomAttributeProvider attributeProvider, bool throwOnDuplicateRegistration = true)
        {
            if (property == null) property = new DotvvmProperty();

            property.Name = propertyName;
            property.IsValueInherited = isValueInherited;
            property.DeclaringType = declaringType;
            property.PropertyType = propertyType;
            property.DefaultValue = defaultValue;
            property.AttributeProvider = attributeProvider;

            return Register(property, throwOnDuplicateRegistration);
        }

        internal static DotvvmProperty Register(DotvvmProperty property, bool throwOnDuplicateRegistration = true)
        {
            InitializeProperty(property);

            var key = (property.DeclaringType, property.Name);
            if (!registeredProperties.TryAdd(key, property))
            {
                if (throwOnDuplicateRegistration)
                    throw new ArgumentException($"Property is already registered: {property.FullName}");
                else
                    property = registeredProperties[key];
            }

            return property;
        }

        public static void InitializeProperty(DotvvmProperty property, ICustomAttributeProvider? attributeProvider = null)
        {
            if (string.IsNullOrWhiteSpace(property.Name))
                throw new Exception("DotvvmProperty must not have empty name.");
            if (property.DeclaringType is null || property.PropertyType is null)
                throw new Exception($"DotvvmProperty {property.DeclaringType?.Name}.{property.Name} must have PropertyType and DeclaringType.");


            property.PropertyInfo ??= property.DeclaringType.GetProperty(property.Name);
            property.AttributeProvider ??=
                attributeProvider ??
                property.PropertyInfo ??
                throw new ArgumentNullException(nameof(attributeProvider));
            property.MarkupOptions ??=
                property.GetAttributes<MarkupOptionsAttribute>().SingleOrDefault()
                ?? new MarkupOptionsAttribute();
            if (string.IsNullOrEmpty(property.MarkupOptions.Name))
                property.MarkupOptions.Name = property.Name;

            property.DataContextChangeAttributes ??=
                property.GetAttributes<DataContextChangeAttribute>().ToArray();
            property.DataContextManipulationAttribute ??=
                property.GetAttributes<DataContextStackManipulationAttribute>().SingleOrDefault();
            if (property.DataContextManipulationAttribute != null && property.DataContextChangeAttributes.Any())
                throw new ArgumentException($"{nameof(DataContextChangeAttributes)} and {nameof(DataContextManipulationAttribute)} can not be set both at property '{property.FullName}'.");
            property.IsBindingProperty = typeof(IBinding).IsAssignableFrom(property.PropertyType);
        }

        public static void CheckAllPropertiesAreRegistered(Type controlType)
        {
            var properties =
               (from p in controlType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where !registeredProperties.ContainsKey((p.DeclaringType!, p.Name))
                where p.GetCustomAttribute<PropertyGroupAttribute>() == null
                let markupOptions = p.GetCustomAttribute<MarkupOptionsAttribute>()
                where markupOptions != null && markupOptions.MappingMode != MappingMode.Exclude
                select p).ToArray();

            if (properties.Any())
            {
                var controlHasOtherProperties = registeredProperties.Values.Any(p => p.DeclaringType == controlType);
                var explicitLoadingHelp =
                    !controlHasOtherProperties ? $" The control does not have any other properties registered. It could indicate that DotVVM did not register properties from this assembly - it is common with ExplicitAssemblyLoading enabled, try to register {controlType.Assembly.GetName().Name} into config.Markup.Assemblies." : "";
                var deprecationHelp = " DotVVM version <= 3.x did support this, but this feature was removed as it lead to many issues. Please register the property using DotvvmProperty.Register. If you find this annoyingly verbose, you could use control capabilities instead (using DotvvmCapabilityProperty.Register).";
                throw new NotSupportedException($"Control '{controlType.Name}' has properties that are not registered as a DotvvmProperty but have a MarkupOptionsAttribute: {string.Join(", ", properties.Select(p => p.Name))}." + explicitLoadingHelp + deprecationHelp);
            }
        }

        private static ConcurrentDictionary<(Type, string), DotvvmProperty> registeredProperties = new();

        /// <summary>
        /// Resolves the <see cref="DotvvmProperty"/> by the declaring type and name.
        /// </summary>
        public static DotvvmProperty? ResolveProperty(Type type, string name)
        {
            DotvvmProperty? property;
            while (!registeredProperties.TryGetValue((type, name), out property) && type.BaseType != null)
            {
                type = type.BaseType;
            }
            return property;
        }

        /// <summary>
        /// Resolves the <see cref="DotvvmProperty"/> from the full name (DeclaringTypeName.PropertyName).
        /// </summary>
        public static DotvvmProperty? ResolveProperty(string fullName, bool caseSensitive = true)
        {
            return registeredProperties.Values.LastOrDefault(p =>
                p.FullName.Equals(fullName, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Resolves all properties of specified type.
        /// </summary>
        public static DotvvmProperty[] ResolveProperties(Type type)
        {
            var types = new HashSet<Type>();
            while (type.BaseType != null)
            {
                types.Add(type);
                type = type.BaseType;
            }

            return registeredProperties.Values.Where(p => types.Contains(p.DeclaringType)).ToArray();
        }

        public override string ToString()
        {
            return $"{this.FullName}";
        }
    }
}
