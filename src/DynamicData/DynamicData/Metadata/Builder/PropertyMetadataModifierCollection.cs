using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DotVVM.Framework.Controls.DynamicData.Metadata.Builder
{
    public class PropertyMetadataModifierCollection
    {
        private List<(Func<PropertyInfo, bool> matcher, Action<PropertyDisplayMetadataModifier> rule)> rules = new();

        public PropertyMetadataModifierCollection For(Type propertyType, Action<PropertyDisplayMetadataModifier> rule)
        {
            return For(p => p.PropertyType == propertyType, rule);
        }

        public PropertyMetadataModifierCollection For(string propertyName, Action<PropertyDisplayMetadataModifier> rule)
        {
            return For(p => p.Name == propertyName, rule);
        }

        public PropertyMetadataModifierCollection For(Func<PropertyInfo, bool> matcher, Action<PropertyDisplayMetadataModifier> rule)
        {
            rules.Add((matcher, rule));
            return this;
        }


        internal PropertyDisplayMetadata ApplyRules(PropertyDisplayMetadata metadata)
        {
            foreach (var rule in rules)
            {
                if (metadata.PropertyInfo is {} && rule.matcher(metadata.PropertyInfo))
                {
                    var modifier = new PropertyDisplayMetadataModifier();
                    rule.rule(modifier);
                    modifier.ApplyModifiers(metadata);
                }
            }
            return metadata;
        }

    }
}
