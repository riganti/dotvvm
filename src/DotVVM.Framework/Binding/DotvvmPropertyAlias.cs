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
                        "not been resolved yet.");
                }
                return aliased;
            }
        }

        public static void Resolve(DotvvmPropertyAlias aliasProperty)
        {
            aliasProperty.aliased = DotvvmProperty.ResolveProperty(
                aliasProperty.AliasedPropertyDeclaringType,
                aliasProperty.AliasedPropertyName);
        }

        public override object? GetValue(DotvvmBindableObject control, bool inherit = true)
        {
            throw new NotSupportedException($"'{FullName}' is a property alias and doesn't support "
                + $"'{nameof(GetValue)}'. Use '{Aliased.FullName}' instead.");
        }

        public override bool IsSet(DotvvmBindableObject control, bool inherit = true)
        {
            throw new NotSupportedException($"'{FullName}' is a property alias and doesn't support "
                + $"'{nameof(IsSet)}'. Use '{Aliased.FullName}' instead.");
        }

        public override void SetValue(DotvvmBindableObject control, object? value)
        {
            throw new NotSupportedException($"'{FullName}' is a property alias and doesn't support "
                + $"'{nameof(SetValue)}'. Use '{Aliased.FullName}' instead.");
        }
    }
}
