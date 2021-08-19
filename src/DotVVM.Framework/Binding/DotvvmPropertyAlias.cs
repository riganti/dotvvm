#nullable enable

using System;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    public class DotvvmPropertyAlias : DotvvmProperty
    {
        private DotvvmProperty? aliased;

        public DotvvmPropertyAlias(
            string aliasName,
            Type declaringType,
            string aliasedPropertyName,
            Type aliasedPropertyDeclaringType)
        {
            Name = aliasName;
            DeclaringType = declaringType;
            AliasedPropertyName = aliasedPropertyName;
            AliasedPropertyDeclaringType = aliasedPropertyDeclaringType;
        }

        public string AliasedPropertyName { get; }
        public Type AliasedPropertyDeclaringType { get; }
        public DotvvmProperty Aliased
        {
            get
            {
                if (aliased == null)
                {
                    throw new NotSupportedException($"The '{FullName}' property alias has " +
                        "not been initialized yet.");
                }
                return aliased;
            }
        }
        public override object? DefaultValue { get => Aliased.DefaultValue; }
        public override Type PropertyType { get => Aliased.PropertyType; }
        public override bool IsValueInherited { get => Aliased.IsValueInherited; }
        public override MarkupOptionsAttribute MarkupOptions
        {
            get => Aliased.MarkupOptions;
            set => Aliased.MarkupOptions = value;
        }
        public override bool IsBindingProperty { get => Aliased.IsBindingProperty; }

        public static void Resolve(DotvvmPropertyAlias aliasProperty)
        {
            aliasProperty.aliased = DotvvmProperty.ResolveProperty(
                aliasProperty.AliasedPropertyDeclaringType,
                aliasProperty.AliasedPropertyName);
        }

        public override object? GetValue(DotvvmBindableObject control, bool inherit = true)
        {
            return Aliased.GetValue(control, inherit);
        }

        public override bool IsSet(DotvvmBindableObject control, bool inherit = true)
        {
            return Aliased.IsSet(control, inherit);
        }

        public override void SetValue(DotvvmBindableObject control, object? value)
        {
            Aliased.SetValue(control, value);
        }
    }
}
