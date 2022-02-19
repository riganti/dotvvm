using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using DotVVM.Framework.Controls.DynamicData.Annotations;

namespace DotVVM.Framework.Controls.DynamicData.Metadata
{
    /// <summary>
    /// Provides a list of properties for the specified entity.
    /// </summary>
    public class DefaultEntityPropertyListProvider : IEntityPropertyListProvider
    {
        private readonly IPropertyDisplayMetadataProvider propertyDisplayMetadataProvider;
        private readonly ConcurrentDictionary<TypeCulturePair, List<PropertyDisplayMetadata>> cache = new ConcurrentDictionary<TypeCulturePair, List<PropertyDisplayMetadata>>();

        
        public DefaultEntityPropertyListProvider(IPropertyDisplayMetadataProvider propertyDisplayMetadataProvider)
        {
            this.propertyDisplayMetadataProvider = propertyDisplayMetadataProvider;
        }

        /// <summary>
        /// Gets a list of properties for the specified entity and view name.
        /// </summary>
        public IList<PropertyDisplayMetadata> GetProperties(Type entityType, IViewContext viewContext)
        {
            var allProperties = cache.GetOrAdd(new TypeCulturePair(entityType, CultureInfo.CurrentUICulture), GetPropertiesCore);
            return FilterProperties(allProperties, viewContext);
        }
        
        private List<PropertyDisplayMetadata> GetPropertiesCore(TypeCulturePair pair)
        {
            var metadata = pair.EntityType.GetTypeInfo().GetProperties()
                .Select(propertyDisplayMetadataProvider.GetPropertyMetadata)
                .OrderBy(p => p.Order)
                .Where(p => p.AutoGenerateField)
                .ToList();

            foreach (var property in metadata)
            {
                if (property.DisplayName is null)
                {
                    property.DisplayName = LocalizableString.Constant(property.PropertyInfo.Name);
                }
            }
            
            return metadata;
        }


        private IList<PropertyDisplayMetadata> FilterProperties(List<PropertyDisplayMetadata> allProperties, IViewContext viewContext)
        {
            return allProperties.Where(p => EvaluatePropertyVisibility(p, viewContext)).ToList();
        }

        private bool EvaluatePropertyVisibility(PropertyDisplayMetadata property, IViewContext viewContext)
        {
            if (property.VisibilityFilters.Length == 0)
            {
                return true;
            }

            foreach (var filter in property.VisibilityFilters)
            {
                var mode = filter.CanShow(viewContext);
                if (mode == VisibilityMode.Show)
                {
                    return true;
                }
                else if (mode == VisibilityMode.Hide)
                {
                    return false;
                }
            }
            return false;
        }


        private struct TypeCulturePair
        {
            public Type EntityType;
            public CultureInfo Culture;

            public TypeCulturePair(Type entityType, CultureInfo culture)
            {
                EntityType = entityType;
                Culture = culture;
            }
        }

    }
}
