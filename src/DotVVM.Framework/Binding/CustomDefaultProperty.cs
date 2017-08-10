using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    public class CustomDefaultProperty : DotvvmProperty
    {
        /// <summary>
        /// Gets the property whose value this property will default to if it is not set
        /// </summary>
        public DotvvmProperty DefaultProperty { get; protected set; }

        /// <summary>
        /// Gets whether the value of the default property when can be inherited from the parent controls.
        /// </summary>
        public bool DefaultPropertyInherit { get; protected set; }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public override object GetValue(DotvvmBindableObject control, bool inherit = true)
        {
            object value;
            if (control.properties != null)
            {
                if (control.properties.TryGetValue(this, out value))
                {
                    return value;
                }
            }
            if (IsValueInherited && inherit && control.Parent != null)
            {
                return GetValue(control.Parent);
            }

            return DefaultProperty.GetValue(control, DefaultPropertyInherit);
        }

        /// <summary>
        /// Gets whether the value of the property is set
        /// </summary>
        public override bool IsSet(DotvvmBindableObject control, bool inherit = true)
        {
            if (control.properties != null && control.properties.ContainsKey(this))
            {
                return true;
            }

            if (IsValueInherited && inherit && control.Parent != null)
            {
                return IsSet(control.Parent);
            }

            return DefaultProperty.IsSet(control, DefaultPropertyInherit);
        }

        /// <summary>
        /// Registers the specified DotVVM property.
        /// </summary>
        public static CustomDefaultProperty Register<TPropertyType, TDeclaringType>(string propertyName, DotvvmProperty defaultProperty, bool defaultPropertyInherit, bool isValueInherited = false)
        {
            var property = new CustomDefaultProperty() { DefaultProperty = defaultProperty, DefaultPropertyInherit = defaultPropertyInherit };
            return DotvvmProperty.Register<TPropertyType, TDeclaringType>(propertyName, isValueInherited: isValueInherited, property: property) as CustomDefaultProperty;
        }
    }
}
