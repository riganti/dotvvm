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

        private readonly ConcurrentDictionary<PropertyCulturePair, PropertyDisplayMetadata> cache = new ConcurrentDictionary<PropertyCulturePair, PropertyDisplayMetadata>();

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
            return cache.GetOrAdd(new PropertyCulturePair(property, CultureInfo.CurrentUICulture), GetPropertyMetadataCore);
        }


        private PropertyDisplayMetadata GetPropertyMetadataCore(PropertyCulturePair pair)
        {
            var metadata = basePropertyDisplayMetadataProvider.GetPropertyMetadata(pair.PropertyInfo);

            if (string.IsNullOrEmpty(metadata.DisplayName))
            {
                metadata.DisplayName = propertyDisplayNames.GetString(pair.PropertyInfo.DeclaringType.Name + "_" + pair.PropertyInfo.Name)
                                       ?? propertyDisplayNames.GetString(pair.PropertyInfo.Name);
            }

            return metadata;
        }


        private struct PropertyCulturePair
        {
            public CultureInfo Culture;
            public PropertyInfo PropertyInfo;

            public PropertyCulturePair(PropertyInfo propertyInfo, CultureInfo culture)
            {
                Culture = culture;
                PropertyInfo = propertyInfo;
            }
        }

    }
}