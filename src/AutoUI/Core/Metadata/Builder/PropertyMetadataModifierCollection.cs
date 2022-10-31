using System;
using System.Collections.Generic;
using System.Reflection;

namespace DotVVM.AutoUI.Metadata.Builder
{
    public class PropertyMetadataModifierCollection
    {
        private readonly List<(Func<PropertyInfo, bool> matcher, Action<PropertyDisplayMetadataModifier> rule)> rules = new();

        /// <summary>
        /// Registers a configuration rule for all properties of the specified type.
        /// </summary>
        public PropertyMetadataModifierCollection For(Type propertyType, Action<PropertyDisplayMetadataModifier> rule)
        {
            return For(p => p.PropertyType == propertyType, rule);
        }

        /// <summary>
        /// Registers a configuration rule for all properties with the specified name.
        /// </summary>
        public PropertyMetadataModifierCollection For(string propertyName, Action<PropertyDisplayMetadataModifier> rule)
        {
            return For(p => p.Name == propertyName, rule);
        }

        /// <summary>
        /// Registers a configuration rule for all properties matching the specified condition.
        /// </summary>
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
