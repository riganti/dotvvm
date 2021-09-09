using System;

namespace DotVVM.Framework.Binding
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PropertyAliasAttribute : Attribute
    {
        public PropertyAliasAttribute(string aliasedPropertyName)
        {
            AliasedPropertyName = aliasedPropertyName;
        }

        public PropertyAliasAttribute(string aliasedPropertyName, Type aliasedPropertyDeclaringType)
        {
            AliasedPropertyName = aliasedPropertyName;
            AliasedPropertyDeclaringType = aliasedPropertyDeclaringType;
        }

        public string AliasedPropertyName { get; }

        public Type? AliasedPropertyDeclaringType { get; }
    }
}
