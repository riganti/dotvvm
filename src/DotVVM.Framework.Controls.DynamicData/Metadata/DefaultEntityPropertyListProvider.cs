using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace DotVVM.Framework.Controls.DynamicData.Metadata
{
    /// <summary>
    /// Provides a list of properties for the specified entity.
    /// </summary>
    public class DefaultEntityPropertyListProvider : IEntityPropertyListProvider
    {
        private readonly IPropertyDisplayMetadataProvider propertyDisplayMetadataProvider;
        private readonly ConcurrentDictionary<TypeViewCulturePair, List<PropertyDisplayMetadata>> cache = new ConcurrentDictionary<TypeViewCulturePair, List<PropertyDisplayMetadata>>();

        
        public DefaultEntityPropertyListProvider(IPropertyDisplayMetadataProvider propertyDisplayMetadataProvider)
        {
            this.propertyDisplayMetadataProvider = propertyDisplayMetadataProvider;
        }

        /// <summary>
        /// Gets a list of properties for the specified entity and view name.
        /// </summary>
        public IList<PropertyDisplayMetadata> GetProperties(Type entityType, string viewName = null)
        {
            return cache.GetOrAdd(new TypeViewCulturePair(entityType, viewName, CultureInfo.CurrentUICulture), GetPropertiesCore);
        }

        private List<PropertyDisplayMetadata> GetPropertiesCore(TypeViewCulturePair pair)
        {
            var metadata = pair.EntityType.GetTypeInfo().GetProperties()
                .Select(propertyDisplayMetadataProvider.GetPropertyMetadata)
                .OrderBy(p => p.Order)
                .Where(p => p.AutoGenerateField)
                .ToList();

            foreach (var property in metadata)
            {
                if (string.IsNullOrEmpty(property.DisplayName))
                {
                    property.DisplayName = property.PropertyInfo.Name;
                }
            }
            
            // TODO: filter by view

            return metadata;
        }

        private struct TypeViewCulturePair
        {
            public Type EntityType;
            public string ViewName;
            public CultureInfo Culture;

            public TypeViewCulturePair(Type entityType, string viewName, CultureInfo culture)
            {
                EntityType = entityType;
                ViewName = viewName;
                Culture = culture;
            }
        }

    }
}