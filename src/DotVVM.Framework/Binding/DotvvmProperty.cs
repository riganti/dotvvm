using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding
{
    /// <summary>
    /// Represents a property of DotVVM controls.
    /// </summary>
    public class DotvvmProperty
    {

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; private set; }

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
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// Gets or sets the markup options.
        /// </summary>
        public MarkupOptionsAttribute MarkupOptions { get; set; }

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


        /// <summary>
        /// Prevents a default instance of the <see cref="DotvvmProperty"/> class from being created.
        /// </summary>
        internal DotvvmProperty()
        {
        }


        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        internal virtual object GetValue(DotvvmControl dotvvmControl, bool inherit = true)
        {
            object value;
            if (dotvvmControl.properties != null && dotvvmControl.properties.TryGetValue(this, out value))
            {
                return value;
            }
            if (IsValueInherited && inherit && dotvvmControl.Parent != null)
            {
                return GetValue(dotvvmControl.Parent);
            }
            return DefaultValue;
        }

        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        internal virtual void SetValue(DotvvmControl dotvvmControl, object value)
        {
            dotvvmControl.Properties[this] = value;
        }


        /// <summary>
        /// Registers the specified DotVVM property.
        /// </summary>
        public static DotvvmProperty Register<TPropertyType, TDeclaringType>(Expression<Func<TDeclaringType, object>> propertyName, object defaultValue = null, bool isValueInherited = false)
        {
            return Register<TPropertyType, TDeclaringType>(ReflectionUtils.GetPropertyNameFromExpression(propertyName), defaultValue, isValueInherited);
        }

        /// <summary>
        /// Registers the specified DotVVM property.
        /// </summary>
        public static DotvvmProperty Register<TPropertyType, TDeclaringType>(string propertyName, object defaultValue = null, bool isValueInherited = false)
        {
            var fullName = typeof(TDeclaringType).FullName + "." + propertyName;

            return registeredProperties.GetOrAdd(fullName, _ =>
            {
                var propertyInfo = typeof(TDeclaringType).GetProperty(propertyName);
                var markupOptions = (propertyInfo != null ? propertyInfo.GetCustomAttribute<MarkupOptionsAttribute>() : null) ?? new MarkupOptionsAttribute()
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

                return new DotvvmProperty()
                {
                    Name = propertyName,
                    DefaultValue = defaultValue ?? default(TPropertyType),
                    DeclaringType = typeof(TDeclaringType),
                    PropertyType = typeof(TPropertyType),
                    IsValueInherited = isValueInherited,
                    PropertyInfo = propertyInfo,
                    MarkupOptions = markupOptions
                };
            });
        }

        /// <summary>
        /// Registers the specified DotVVM property.
        /// </summary>
        public static DotvvmProperty RegisterControlStateProperty<TPropertyType, TDeclaringType>(Expression<Func<TDeclaringType, object>> propertyName, object defaultValue = null)
        {
            return RegisterControlStateProperty<TPropertyType, TDeclaringType>(ReflectionUtils.GetPropertyNameFromExpression(propertyName), defaultValue);
        }

        /// <summary>
        /// Registers the specified DotVVM property.
        /// </summary>
        public static DotvvmProperty RegisterControlStateProperty<TPropertyType, TDeclaringType>(string propertyName, object defaultValue = null)
        {
            return Register<TPropertyType, TDeclaringType>(propertyName, defaultValue: new ControlStateBindingExpression(propertyName) { DefaultValue = defaultValue ?? default(TPropertyType) });
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
        public static DotvvmProperty ResolveProperty(string fullName)
        {
            return registeredProperties.Values.LastOrDefault(p => p.FullName == fullName);
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
        protected internal virtual void OnControlInitialized(DotvvmControl dotvvmControl)
        {
            if (DefaultValue is ControlStateBindingExpression && dotvvmControl is DotvvmBindableControl)
            {
                // register the default value to the control state
                var bindableControl = (DotvvmBindableControl)dotvvmControl;
                if (bindableControl.RequiresControlState)
                {
                    if (bindableControl.GetBinding(this) == null)
                    {
                        bindableControl.SetBinding(this, (ControlStateBindingExpression)DefaultValue);
                    }

                    if (PropertyInfo.SetMethod == null)
                    {
                        var value = PropertyInfo.GetValue(dotvvmControl);
                        ((ControlStateBindingExpression)DefaultValue).UpdateSource(value, bindableControl, this);
                    }
                }
            }
        }

        /// <summary>
        /// Called right before the page is rendered.
        /// </summary>
        public void OnControlRendering(DotvvmControl dotvvmControl)
        {
            if (DefaultValue is ControlStateBindingExpression && dotvvmControl is DotvvmBindableControl)
            {
                // save the property value to the control state
                var bindableControl = (DotvvmBindableControl)dotvvmControl;
                if (bindableControl.RequiresControlState)
                {
                    var value = PropertyInfo.GetValue(dotvvmControl);
                    ((ControlStateBindingExpression)DefaultValue).UpdateSource(value, bindableControl, this);
                }
            }
        }
    }
}