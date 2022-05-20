using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.AutoUI.Annotations;

namespace DotVVM.AutoUI.Metadata
{
    /// <summary>
    /// Provides a list of properties for the specified entity.
    /// </summary>
    public class DefaultEntityPropertyListProvider : IEntityPropertyListProvider
    {
        private readonly IPropertyDisplayMetadataProvider propertyDisplayMetadataProvider;
        
        public DefaultEntityPropertyListProvider(IPropertyDisplayMetadataProvider propertyDisplayMetadataProvider)
        {
            this.propertyDisplayMetadataProvider = propertyDisplayMetadataProvider;
        }

        /// <summary>
        /// Gets a list of properties for the specified entity and view name.
        /// </summary>
        public IEnumerable<PropertyDisplayMetadata> GetProperties(Type entityType, IViewContext viewContext)
        {
            var metadata = entityType.GetTypeInfo().GetProperties()
                .Select(propertyDisplayMetadataProvider.GetPropertyMetadata)
                .OrderBy(p => p.Order)
                .Where(p => p.AutoGenerateField)
                .Where(p => string.IsNullOrEmpty(viewContext.GroupName)
                            || viewContext.GroupName.Equals(p.GroupName, StringComparison.OrdinalIgnoreCase))
                .Where(p => string.IsNullOrEmpty(viewContext.ViewName)
                        || p.VisibleAttributes.All(a =>
                            ConditionalFieldBindingProvider.ProcessExpression(a.ViewNames,
                                v => v.Equals(viewContext.ViewName, StringComparison.OrdinalIgnoreCase))))
                .ToList();
            
            return metadata;
        }
    }
}
