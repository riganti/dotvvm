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
        public bool IsResolved => aliased is object;

        public static void Resolve(DotvvmPropertyAlias alias)
        {
            var aliased = DotvvmProperty.ResolveProperty(
                alias.AliasedPropertyDeclaringType,
                alias.AliasedPropertyName);
            if (aliased is null)
            {
                throw new ArgumentException($"Property alias '{alias}' could not be resolved. "
                    + $"The aliased property '{alias.AliasedPropertyDeclaringType.Name}.{alias.AliasedPropertyName}' is not registered.");
            }

            alias.aliased = aliased;
            alias.DefaultValue = aliased.DefaultValue;
            alias.PropertyType = aliased.PropertyType;
            alias.IsValueInherited = aliased.IsValueInherited;
            alias.MarkupOptions = aliased.MarkupOptions;
            alias.IsVirtual = aliased.IsVirtual;
            alias.IsBindingProperty = aliased.IsBindingProperty;
            alias.DataContextChangeAttributes = aliased.DataContextChangeAttributes;
            alias.DataContextManipulationAttribute = aliased.DataContextManipulationAttribute;
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
