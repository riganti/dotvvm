using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using DotVVM.Framework.Controls.DynamicData.Annotations;
using DotVVM.Framework.Controls.DynamicData.Configuration;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls.DynamicData.Metadata
{
    /// <summary>
    /// Provides information about how the property is displayed in the user interface.
    /// </summary>
    public class DataAnnotationsPropertyDisplayMetadataProvider : IPropertyDisplayMetadataProvider
    {
        private readonly DynamicDataConfiguration dynamicDataConfiguration;

        private readonly ConcurrentDictionary<PropertyCulturePair, PropertyDisplayMetadata> cache = new ConcurrentDictionary<PropertyCulturePair, PropertyDisplayMetadata>();

        public DataAnnotationsPropertyDisplayMetadataProvider(DynamicDataConfiguration dynamicDataConfiguration)
        {
            this.dynamicDataConfiguration = dynamicDataConfiguration;
        }

        /// <summary>
        /// Get the metadata about how the property is displayed.
        /// </summary>
        public PropertyDisplayMetadata GetPropertyMetadata(PropertyInfo property)
        {
            return cache.GetOrAdd(
                new PropertyCulturePair(property, CultureInfo.CurrentUICulture),
                p => dynamicDataConfiguration.PropertyMetadataRules.ApplyRules(GetPropertyMetadataCore(p)));
        }

        private PropertyDisplayMetadata GetPropertyMetadataCore(PropertyCulturePair pair)
        {
            var displayAttribute = pair.PropertyInfo.GetCustomAttribute<DisplayAttribute>();
            var displayFormatAttribute = pair.PropertyInfo.GetCustomAttribute<DisplayFormatAttribute>();
            var dataTypeAttribute = pair.PropertyInfo.GetCustomAttribute<DataTypeAttribute>();
            var styleAttribute = pair.PropertyInfo.GetCustomAttribute<StyleAttribute>();
            var editableFilter = pair.PropertyInfo.GetCustomAttribute<EditableAttribute>();
            var selectorAttribute = pair.PropertyInfo.GetCustomAttribute<SelectorAttribute>();

            return new PropertyDisplayMetadata(pair.PropertyInfo)
            {
                DisplayName = string.IsNullOrEmpty(displayAttribute?.Name) ? null : LocalizableString.Create(displayAttribute.Name, displayAttribute.ResourceType),
                Placeholder = string.IsNullOrEmpty(displayAttribute?.Prompt) ? null : LocalizableString.Create(displayAttribute.Prompt, displayAttribute.ResourceType),
                Description = string.IsNullOrEmpty(displayAttribute?.Description) ? null : LocalizableString.Create(displayAttribute.Description, displayAttribute.ResourceType),
                Order = displayAttribute?.GetOrder(),
                GroupName = displayAttribute?.GetGroupName(),
                FormatString = displayFormatAttribute?.DataFormatString,
                NullDisplayText = displayFormatAttribute?.NullDisplayText,
                AutoGenerateField = displayAttribute?.GetAutoGenerateField() ?? true,
                VisibleAttributes = pair.PropertyInfo.GetCustomAttributes<VisibleAttribute>(),
                DataType = dataTypeAttribute?.DataType,
                Styles = styleAttribute,
                IsEditable = editableFilter?.AllowEdit != false,
                EnabledAttributes = pair.PropertyInfo.GetCustomAttributes<EnabledAttribute>(),
                SelectorConfiguration = selectorAttribute,
                IsDefaultLabelAllowed = pair.PropertyInfo.PropertyType.UnwrapNullableType() != typeof(bool) // TODO: make this configurable, maybe?
            };
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
