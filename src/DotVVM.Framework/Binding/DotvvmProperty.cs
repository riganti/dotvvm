using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using System.Diagnostics;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using Newtonsoft.Json;

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
        public string Name { get; private set; }

        [JsonIgnore]
        ITypeDescriptor IPropertyDescriptor.DeclaringType => new ResolvedTypeDescriptor(DeclaringType);

        [JsonIgnore]
        ITypeDescriptor IPropertyDescriptor.PropertyType => new ResolvedTypeDescriptor(PropertyType);

        /// <summary>
        /// Gets the default value of the property.
        /// </summary>
        public object DefaultValue { get; private set; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        public Type PropertyType { get; private set; }

        /// <summary>
        /// Gets the type of the class where the property is registered.
        /// </summary>
        public Type DeclaringType { get; private set; }

        /// <summary>
        /// Gets whether the value can be inherited from the parent controls.
        /// </summary>
        public bool IsValueInherited { get; private set; }

        /// <summary>
        /// Gets or sets the Reflection property information.
        /// </summary>
        [JsonIgnore]
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Gets or sets the markup options.
        /// </summary>
        public MarkupOptionsAttribute MarkupOptions { get; set; }

        /// <summary>
        /// Virtual DotvvmProperty are not explicitly registred but marked with [MarkupOptions] attribute on DotvvmControl
        /// </summary>
        public bool IsVirtual { get; set; }

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

        /// <summary>
        /// Prevents a default instance of the <see cref="DotvvmProperty"/> class from being created.
        /// </summary>
        internal DotvvmProperty()
        {
        }


        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public virtual object GetValue(DotvvmBindableObject control, bool inherit = true)
        {
            object value;
            if (control.properties != null && control.properties.TryGetValue(this, out value))
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
            if (control.properties != null && control.properties.ContainsKey(this))
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
        public virtual void SetValue(DotvvmBindableObject control, object value)
        {
            control.Properties[this] = value;
        }


        /// <summary>
        /// Registers the specified DotVVM property.
        /// </summary>
        public static DotvvmProperty Register<TPropertyType, TDeclaringType>(Expression<Func<TDeclaringType, object>> propertyName, TPropertyType defaultValue = default(TPropertyType), bool isValueInherited = false)
        {
            return Register<TPropertyType, TDeclaringType>(ReflectionUtils.GetPropertyNameFromExpression(propertyName), defaultValue, isValueInherited);
        }

        /// <summary>
        /// Registers the specified DotVVM property.
        /// </summary>
        public static DotvvmProperty Register<TPropertyType, TDeclaringType>(string propertyName, TPropertyType defaultValue = default(TPropertyType), bool isValueInherited = false, DotvvmProperty property = null)
        {
            var fullName = typeof(TDeclaringType).FullName + "." + propertyName;
            var field = typeof (TDeclaringType).GetField(propertyName + "Property", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            return registeredProperties.GetOrAdd(fullName, _ =>
            {
                var propertyInfo = typeof(TDeclaringType).GetProperty(propertyName);
                var markupOptions = propertyInfo?.GetCustomAttribute<MarkupOptionsAttribute>() 
                    ?? field?.GetCustomAttribute<MarkupOptionsAttribute>()
                    ?? new MarkupOptionsAttribute()
                {
                    AllowBinding = true,
                    AllowHardCodedValue = true,
                    MappingMode = MappingMode.Attribute,
                    Name = propertyName
                };
                if (string.IsNullOrEmpty(markupOptions.Name))
                {
                    markupOptions.Name = propertyName;
                }

                if (property == null) property = new DotvvmProperty();
                property.Name = propertyName;
                property.DefaultValue = defaultValue;
                property.DeclaringType = typeof(TDeclaringType);
                property.PropertyType = typeof(TPropertyType);
                property.IsValueInherited = isValueInherited;
                property.PropertyInfo = propertyInfo;
                property.DataContextChangeAttributes = (propertyInfo != null ? propertyInfo.GetCustomAttributes<DataContextChangeAttribute>(true) : field.GetCustomAttributes<DataContextChangeAttribute>()).ToArray();
                property.MarkupOptions = markupOptions;
                property.IsBindingProperty = typeof(IBinding).IsAssignableFrom(property.PropertyType);
                return property;
            });
        }


        public static IEnumerable<DotvvmProperty> GetVirtualProperties(Type controlType)
            => from p in controlType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
               where !registeredProperties.ContainsKey(p.DeclaringType.FullName + "." + p.Name)
               let markupOptions = GetVirtualPropertyMarkupOptions(p)
               where markupOptions != null
               where markupOptions.MappingMode != MappingMode.Exclude
               select new DotvvmProperty
               {
                   DeclaringType = controlType,
                   IsValueInherited = false,
                   MarkupOptions = markupOptions,
                   Name = p.Name,
                   PropertyInfo = p,
                   PropertyType = p.PropertyType,
                   IsVirtual = true
               };

        private static MarkupOptionsAttribute GetVirtualPropertyMarkupOptions(PropertyInfo p)
        {
            var mo = p.GetCustomAttribute<MarkupOptionsAttribute>();
            if (mo == null) return null;
            mo.AllowBinding = false;
            return mo;
        }

        private static ConcurrentDictionary<string, DotvvmProperty> registeredProperties = new ConcurrentDictionary<string, DotvvmProperty>();

        /// <summary>
        /// Resolves the <see cref="DotvvmProperty"/> by the declaring type and name.
        /// </summary>
        public static DotvvmProperty ResolveProperty(Type type, string name)
        {
            var fullName = type.FullName + "." + name;

            DotvvmProperty property;
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
        public static DotvvmProperty ResolveProperty(string fullName, bool caseSensitive = true)
        {
            return registeredProperties.Values.LastOrDefault(p => 
                p.FullName.Equals(fullName, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Resolves all properties of specified type.
        /// </summary>
        public static IReadOnlyList<DotvvmProperty> ResolveProperties(Type type)
        {
            var types = new HashSet<Type>();
            while (type.BaseType != null)
            {
                types.Add(type);
                type = type.BaseType;
            }

            return registeredProperties.Values.Where(p => types.Contains(p.DeclaringType)).ToList();
        }

        /// <summary>
        /// Called when a control of the property type is created and initialized.
        /// </summary>
        protected internal virtual void OnControlInitialized(DotvvmBindableObject dotvvmControl)
        {
        }

        /// <summary>
        /// Called right before the page is rendered.
        /// </summary>
        public void OnControlRendering(DotvvmBindableObject dotvvmControl)
        {
        }
    }
}