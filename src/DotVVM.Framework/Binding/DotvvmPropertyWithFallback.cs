using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    /// <summary>
    /// The DotvvmProperty that fallbacks to another DotvvmProperty's value.
    /// </summary>
    public class DotvvmPropertyWithFallback : DotvvmProperty
    {
        /// <summary>
        /// Gets the property which value will be used as a follback when this property is not set.
        /// </summary>
        public DotvvmProperty FallbackProperty { get; protected set; }

        /// <inheritdoc />
        public override object GetValue(DotvvmBindableObject control, bool inherit = true)
        {
            if (!TryGetValue(control, out var value, inherit))
            {
                return FallbackProperty.GetValue(control, inherit);
            }

            return value;
        }

        /// <summary>
        /// Gets whether the value of the property is set
        /// </summary>
        public override bool IsSet(DotvvmBindableObject control, bool inherit = true)
        {
            return base.IsSet(control, inherit) || FallbackProperty.IsSet(control, inherit);
        }

        /// <summary>
        /// Registers a new DotVVM property which fallbacks to the <paramref name="fallbackProperty" /> when not set.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="fallbackProperty">The property which value will be used as a follback when the new property is not set.</param>
        /// <param name="isValueInherited">Indicates whether the value can be inherited from the parent controls.</param>
        public static DotvvmPropertyWithFallback Register<TPropertyType, TDeclaringType>(string propertyName, DotvvmProperty fallbackProperty, bool isValueInherited = false)
        {
            var property = new DotvvmPropertyWithFallback { FallbackProperty = fallbackProperty };
            return Register<TPropertyType, TDeclaringType>(propertyName, isValueInherited: isValueInherited, property: property) as DotvvmPropertyWithFallback;
        }

        private bool TryGetValue(DotvvmBindableObject control, out object value, bool inherit = true)
        {
            if (control.properties != null && control.properties.TryGetValue(this, out value))
            {
                return true;
            }

            if (IsValueInherited && inherit && control.Parent != null)
            {
                return TryGetValue(control.Parent, out value);
            }

            value = null;
            return false;
        }
    }
}
