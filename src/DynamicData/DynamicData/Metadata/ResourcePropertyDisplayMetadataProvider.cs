using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace DotVVM.Framework.Controls.DynamicData.Metadata
{
    /// <summary>
    /// Property display metadata provider which loads missing property names from the RESX file.
    /// </summary>
    public class ResourcePropertyDisplayMetadataProvider : IPropertyDisplayMetadataProvider
    {
        private readonly IPropertyDisplayMetadataProvider basePropertyDisplayMetadataProvider;
        private readonly ResourceManager propertyDisplayNames;
        private readonly ConcurrentDictionary<PropertyInfo, PropertyDisplayMetadata> cache = new();

        /// <summary>
        /// Gets the type of the resource file that contains the property names.
        /// The resource key is in the "TypeName_PropertyName" or "PropertyName".
        /// </summary>
        public Type PropertyDisplayNameResourceFile { get; }


        public ResourcePropertyDisplayMetadataProvider(Type propertyDisplayNameResourceFile, IPropertyDisplayMetadataProvider basePropertyDisplayMetadataProvider)
        {
            this.basePropertyDisplayMetadataProvider = basePropertyDisplayMetadataProvider;

            PropertyDisplayNameResourceFile = propertyDisplayNameResourceFile;
            propertyDisplayNames = new ResourceManager(propertyDisplayNameResourceFile);
        }


        /// <summary>
        /// Get the metadata about how the property is displayed.
        /// </summary>
        public PropertyDisplayMetadata GetPropertyMetadata(PropertyInfo property)
        {
            return cache.GetOrAdd(property, GetPropertyMetadataCore);
        }


        private PropertyDisplayMetadata GetPropertyMetadataCore(PropertyInfo property)
        {
            var metadata = basePropertyDisplayMetadataProvider.GetPropertyMetadata(property);

            if (metadata.DisplayName is null)
            {
                var key1 = property.DeclaringType!.Name + "_" + property.Name;
                var key2 = property.Name;
                if (propertyDisplayNames.GetString(key1) is not null)
                    metadata.DisplayName = LocalizableString.Localized(PropertyDisplayNameResourceFile, key1);
                else if (propertyDisplayNames.GetString(key2) is not null)
                    metadata.DisplayName = LocalizableString.Localized(PropertyDisplayNameResourceFile, key2);
            }

            return metadata;
        }
    }
}
