#nullable enable
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
        public virtual object? DefaultValue { get; protected set; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        public virtual Type PropertyType { get; protected set; }

        /// <summary>
        /// Gets the type of the class where the property is registered.
        /// </summary>
        public Type DeclaringType { get; protected set; }

        /// <summary>
        /// Gets whether the value can be inherited from the parent controls.
        /// </summary>
        public virtual bool IsValueInherited { get; protected set; }

        /// <summary>
        /// Gets or sets the Reflection property information.
        /// </summary>
        [JsonIgnore]
        public PropertyInfo? PropertyInfo { get; private set; }

        /// <summary>
        /// Gets or sets the markup options.
        /// </summary>
        public virtual MarkupOptionsAttribute MarkupOptions { get; set; }

        /// <summary>
        /// Virtual DotvvmProperty are not explicitly registered but marked with [MarkupOptions] attribute on DotvvmControl
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Determines if property type inherits from IBinding
        /// </summary>
        public virtual bool IsBindingProperty { get; private set; }

        /// <summary>
        /// Gets whether this property has been made obsolete.
        /// </summary>
        /// <remarks>
        /// Is set to true if the property has the <see cref="ObsoleteAttribute"/>.
        /// The workaround message from the attribute is contained in <see cref="WorkaroundMessage"/>.
        /// </remarks>
        public bool IsObsolete { get; protected set; }

        /// <summary>
        /// Gets the workaround message from the <see cref="ObsoleteAttribute"/> on the property field
        /// if the attribute exists.
        /// </summary>
        public string? WorkaroundMessage { get; protected set; }

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
        public virtual DataContextChangeAttribute[] DataContextChangeAttributes { get; private set; }

        [JsonIgnore]
        public virtual DataContextStackManipulationAttribute? DataContextManipulationAttribute { get; private set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="DotvvmProperty"/> class from being created.
        /// </summary>
#pragma warning disable CS8618 // DotvvmProperty is usually initialized by InitializeProperty
        internal DotvvmProperty()
#pragma warning restore CS8618
        {
        }


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
            var fullName = declaringType.FullName + "." + propertyName;

            if (property == null) property = new DotvvmProperty();

            property.Name = propertyName;
            property.IsValueInherited = isValueInherited;
            property.DeclaringType = declaringType;
            property.PropertyType = propertyType;
            property.DefaultValue = defaultValue;

            InitializeProperty(property, attributeProvider);

            if (!registeredProperties.TryAdd(fullName, property))
            {
                if (throwOnDuplicateRegistration)
                    throw new ArgumentException($"Property is already registered: {fullName}");
                else
                    property = registeredProperties[fullName];
            }

            return property;
        }

        /// <summary>
        /// Registers an alias with a property accessor for another DotvvmProperty given by the PropertyAlias attribute.
        /// </summary>
        public static DotvvmPropertyAlias RegisterAlias<TDeclaringType>(
            Expression<Func<TDeclaringType, object?>> propertyAccessor)
        {
            var property = ReflectionUtils.GetMemberFromExpression(propertyAccessor.Body) as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException("The expression should be simple property access",
                    nameof(propertyAccessor));
            }
            return RegisterAlias<TDeclaringType>(property.Name);
        }

        /// <summary>
        /// Registers an alias for another DotvvmProperty given by the PropertyAlias attribute.
        /// </summary>
        public static DotvvmPropertyAlias RegisterAlias<TDeclaringType>(
            Expression<Func<DotvvmProperty?>> fieldAccessor)
        {
            var field = ReflectionUtils.GetMemberFromExpression(fieldAccessor) as FieldInfo;
            if (field == null || !field.IsStatic)
            {
                throw new ArgumentException("The expression should be simple static field access",
                    nameof(fieldAccessor));
            }
            if (!field.Name.EndsWith("Property", StringComparison.Ordinal))
            {
                throw new ArgumentException($"DotVVM property backing field's '{field.Name}' "
                    + "name should end with 'Property'");
            }
            return RegisterAlias<TDeclaringType>(field.Name.Remove(field.Name.Length - "Property".Length));
        }

        /// <summary>
        /// Registers an alias for another DotvvmProperty given by the PropertyAlias attribute.
        /// </summary>
        public static DotvvmPropertyAlias RegisterAlias<TDeclaringType>(string aliasName)
        {
            var field = typeof(TDeclaringType).GetField(
                aliasName + "Property",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new ArgumentException($"'{typeof(TDeclaringType).Name}' does not contain static field '{aliasName}Property'.");
            }
            return RegisterAlias(aliasName, typeof(TDeclaringType), field);
        }

        /// <summary>
        /// Registers an alias for a DotvvmProperty.
        /// </summary>
        public static DotvvmPropertyAlias RegisterAlias(
            string aliasName,
            Type declaringType,
            ICustomAttributeProvider attributeProvider,
            bool throwOnDuplicitRegistration = true)
        {
            var propertyInfo = declaringType.GetProperty(aliasName);
            var aliasAttribute = propertyInfo?.GetCustomAttribute<PropertyAliasAttribute>()
                ?? attributeProvider.GetCustomAttribute<PropertyAliasAttribute>();
            if (aliasAttribute is null)
            {
                throw new ArgumentException($"A property alias must have a {nameof(PropertyAliasAttribute)}.");
            }

            var aliasPropertyDeclaringType = aliasAttribute.AliasedPropertyDeclaringType
                ?? declaringType;

            var propertyAlias = new DotvvmPropertyAlias(
                aliasName,
                declaringType,
                aliasAttribute.AliasedPropertyName,
                aliasPropertyDeclaringType);
            InitializeObsoletionMetadata(propertyAlias, propertyInfo, attributeProvider);
            var fullName = propertyAlias.DeclaringType.FullName + "." + propertyAlias.Name;

            if (!registeredProperties.TryAdd(fullName, propertyAlias))
            {
                if (throwOnDuplicitRegistration)
                    throw new ArgumentException($"Property is already registered: {fullName}");
            }

            if (!registeredAliases.TryAdd(fullName, propertyAlias))
            {
                if (throwOnDuplicitRegistration)
                    throw new ArgumentException($"Property alias is already registered: {fullName}");
                else
                    return registeredAliases[fullName];
            }
            return propertyAlias;
        }

        public static void InitializeProperty(
            DotvvmProperty property,
            ICustomAttributeProvider attributeProvider,
            DotvvmProperty? fallbackProperty = default)
        {
            var propertyInfo = property.DeclaringType.GetProperty(property.Name);
            var markupOptions = propertyInfo?.GetCustomAttribute<MarkupOptionsAttribute>()
                ?? attributeProvider.GetCustomAttribute<MarkupOptionsAttribute>()
                ?? fallbackProperty?.MarkupOptions
                ?? new MarkupOptionsAttribute() {
                    AllowBinding = true,
                    AllowHardCodedValue = true,
                    MappingMode = MappingMode.Attribute,
                    Name = property.Name
                };
            if (string.IsNullOrEmpty(markupOptions.Name))
                markupOptions.Name = property.Name;

            if (property == null)
                property = new DotvvmProperty();

            property.PropertyInfo = propertyInfo;
            property.DataContextChangeAttributes = (propertyInfo != null ?
                propertyInfo.GetCustomAttributes<DataContextChangeAttribute>(true) :
                attributeProvider.GetCustomAttributes<DataContextChangeAttribute>()).ToArray();
            if (property.DataContextChangeAttributes.Length == 0 && fallbackProperty != null)
            {
                property.DataContextChangeAttributes = fallbackProperty.DataContextChangeAttributes;
            }

            property.DataContextManipulationAttribute = propertyInfo != null ?
                propertyInfo.GetCustomAttribute<DataContextStackManipulationAttribute>(true) :
                attributeProvider.GetCustomAttribute<DataContextStackManipulationAttribute>() ??
                fallbackProperty?.DataContextManipulationAttribute;
            if (property.DataContextManipulationAttribute != null && property.DataContextChangeAttributes.Any()) {
                throw new ArgumentException($"{nameof(DataContextChangeAttributes)} "
                    + $"and {nameof(DataContextManipulationAttribute)} can not be set both at once on property "
                    + $"'{property.FullName}'.");
            }
            property.MarkupOptions = markupOptions;
            property.IsBindingProperty = typeof(IBinding).IsAssignableFrom(property.PropertyType);

            InitializeObsoletionMetadata(property, propertyInfo, attributeProvider, fallbackProperty);
        }

        public static IEnumerable<DotvvmProperty> GetVirtualProperties(Type controlType)
            => from p in controlType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
               where !registeredProperties.ContainsKey(p.DeclaringType!.FullName + "." + p.Name)
               let markupOptions = GetVirtualPropertyMarkupOptions(p)
               where markupOptions != null
               where p.GetCustomAttribute<PropertyGroupAttribute>() == null
               where markupOptions.MappingMode != MappingMode.Exclude
               select new DotvvmProperty {
                   DeclaringType = controlType,
                   IsValueInherited = false,
                   MarkupOptions = markupOptions,
                   Name = p.Name,
                   PropertyInfo = p,
                   PropertyType = p.PropertyType,
                   IsVirtual = true
               };

        private static MarkupOptionsAttribute? GetVirtualPropertyMarkupOptions(PropertyInfo p)
        {
            var mo = p.GetCustomAttribute<MarkupOptionsAttribute>();
            if (mo == null) return null;
            mo.AllowBinding = false;
            return mo;
        }

        private static void InitializeObsoletionMetadata(
            DotvvmProperty property,
            PropertyInfo? propertyInfo,
            ICustomAttributeProvider attributeProvider,
            DotvvmProperty? fallbackProperty = default)
        {
            var obsoleteAttribute = propertyInfo?.GetCustomAttribute<ObsoleteAttribute>()
                ?? attributeProvider.GetCustomAttribute<ObsoleteAttribute>();
            property.IsObsolete = obsoleteAttribute != null || fallbackProperty?.IsObsolete == true;
            property.WorkaroundMessage = obsoleteAttribute?.Message ?? fallbackProperty?.WorkaroundMessage;
        }

        private static ConcurrentDictionary<string, DotvvmProperty> registeredProperties
            = new ConcurrentDictionary<string, DotvvmProperty>();
        private static ConcurrentDictionary<string, DotvvmPropertyAlias> registeredAliases
            = new ConcurrentDictionary<string, DotvvmPropertyAlias>();

        /// <summary>
        /// Resolves the <see cref="DotvvmProperty"/> by the declaring type and name.
        /// </summary>
        public static DotvvmProperty? ResolveProperty(Type type, string name)
        {
            var fullName = type.FullName + "." + name;

            DotvvmProperty? property;
            while (!registeredProperties.TryGetValue(fullName, out property) && type.BaseType != null)
            {
                type = type.BaseType;
                fullName = type.FullName + "." + name;
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

        public static IReadOnlyDictionary<string, DotvvmProperty> GetRegisteredProperties()
        {
            return ReadOnlyDictionaryWrapper<string, DotvvmProperty>.WrapIfNeeded(registeredProperties);
        }

        public static IReadOnlyDictionary<string, DotvvmPropertyAlias> GetRegisteredAliases()
        {
            return ReadOnlyDictionaryWrapper<string, DotvvmPropertyAlias>.WrapIfNeeded(registeredAliases);
        }

        public override string ToString()
        {
            return $"{this.FullName}";
        }
    }
}
