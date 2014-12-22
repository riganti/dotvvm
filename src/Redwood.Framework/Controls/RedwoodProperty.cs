using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Redwood.Framework.Binding;
using Redwood.Framework.Utils;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Represents a property of Redwood controls.
    /// </summary>
    public class RedwoodProperty
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
        /// Gets the full name of the property.
        /// </summary>
        public string FullName
        {
            get { return PropertyType.Name + "." + Name; }
        }


        /// <summary>
        /// Prevents a default instance of the <see cref="RedwoodProperty"/> class from being created.
        /// </summary>
        internal RedwoodProperty()
        {
        }


        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        internal virtual object GetValue(RedwoodControl redwoodControl, bool inherit = true)
        {
            object value;
            if (redwoodControl.properties != null && redwoodControl.properties.TryGetValue(this, out value))
            {
                return value;
            }
            if (IsValueInherited && inherit && redwoodControl.Parent != null)
            {
                return GetValue(redwoodControl.Parent);
            }
            return DefaultValue;
        }

        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        internal virtual void SetValue(RedwoodControl redwoodControl, object value)
        {
            redwoodControl.Properties[this] = value;
        }


        /// <summary>
        /// Registers the specified Redwood property.
        /// </summary>
        public static RedwoodProperty Register<TPropertyType, TDeclaringType>(Expression<Func<TDeclaringType, object>> propertyName, object defaultValue = null, bool isValueInherited = false)
        {
            return Register<TPropertyType, TDeclaringType>(ReflectionUtils.GetPropertyNameFromExpression(propertyName), defaultValue, isValueInherited);
        }

        /// <summary>
        /// Registers the specified Redwood property.
        /// </summary>
        public static RedwoodProperty Register<TPropertyType, TDeclaringType>(string propertyName, object defaultValue = null, bool isValueInherited = false)
        {
            var fullName = typeof (TDeclaringType).FullName + "." + propertyName;
            
            return registeredProperties.GetOrAdd(fullName, _ => new RedwoodProperty()
            {
                Name = propertyName,
                DefaultValue = defaultValue,
                DeclaringType = typeof(TDeclaringType),
                PropertyType = typeof(TPropertyType),
                IsValueInherited = isValueInherited
            });
        }

        /// <summary>
        /// Registers the specified Redwood property.
        /// </summary>
        public static RedwoodProperty RegisterControlStateProperty<TPropertyType, TDeclaringType>(Expression<Func<TDeclaringType, object>> propertyName)
        {
            return RegisterControlStateProperty<TPropertyType, TDeclaringType>(ReflectionUtils.GetPropertyNameFromExpression(propertyName));
        }

        /// <summary>
        /// Registers the specified Redwood property.
        /// </summary>
        public static RedwoodProperty RegisterControlStateProperty<TPropertyType, TDeclaringType>(string propertyName)
        {
            return Register<TPropertyType, TDeclaringType>(propertyName, defaultValue: new ControlStateBindingExpression(propertyName));
        }


        private static ConcurrentDictionary<string, RedwoodProperty> registeredProperties = new ConcurrentDictionary<string, RedwoodProperty>(); 

        /// <summary>
        /// Resolves the <see cref="RedwoodProperty"/> by the declaring type and name.
        /// </summary>
        public static RedwoodProperty ResolveProperty(Type type, string name)
        {
            var fullName = type.FullName + "." + name;

            RedwoodProperty property;
            while (!registeredProperties.TryGetValue(fullName, out property) && type.BaseType != null)
            {
                type = type.BaseType;
                fullName = type.FullName + "." + name;
            }
            return property;
        }
    }
}