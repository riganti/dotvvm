using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using DotVVM.AutoUI.Annotations;
using DotVVM.AutoUI.Configuration;
using DotVVM.Framework.Utils;

namespace DotVVM.AutoUI.Metadata
{
    /// <summary>
    /// Provides information about how the property is displayed in the user interface.
    /// </summary>
    public class DataAnnotationsPropertyDisplayMetadataProvider : IPropertyDisplayMetadataProvider
    {
        private readonly AutoUIConfiguration autoUiConfiguration;

        private readonly ConcurrentDictionary<PropertyInfo, PropertyDisplayMetadata> cache = new();

        public DataAnnotationsPropertyDisplayMetadataProvider(AutoUIConfiguration autoUiConfiguration)
        {
            this.autoUiConfiguration = autoUiConfiguration;
        }

        /// <summary>
        /// Get the metadata about how the property is displayed.
        /// </summary>
        public PropertyDisplayMetadata GetPropertyMetadata(PropertyInfo property)
        {
            return cache.GetOrAdd(
                property,
                p => autoUiConfiguration.PropertyMetadataRules.ApplyRules(GetPropertyMetadataCore(p)));
        }

        private PropertyDisplayMetadata GetPropertyMetadataCore(PropertyInfo p)
        {
            var displayAttribute = p.GetCustomAttribute<DisplayAttribute>();
            var displayFormatAttribute = p.GetCustomAttribute<DisplayFormatAttribute>();
            var dataTypeAttribute = p.GetCustomAttribute<DataTypeAttribute>();
            var styleAttribute = p.GetCustomAttribute<StyleAttribute>();
            var editableFilter = p.GetCustomAttribute<EditableAttribute>();
            var selectorAttribute = p.GetCustomAttribute<SelectorAttribute>();
            var uiHintAttributes = p.GetCustomAttributes<UIHintAttribute>();

            return new PropertyDisplayMetadata(p)
            {
                DisplayName = LocalizableString.CreateNullable(displayAttribute?.Name, displayAttribute?.ResourceType),
                Placeholder = LocalizableString.CreateNullable(displayAttribute?.Prompt, displayAttribute?.ResourceType),
                Description = LocalizableString.CreateNullable(displayAttribute?.Description, displayAttribute?.ResourceType),
                Order = displayAttribute?.GetOrder(),
                GroupName = displayAttribute?.GetGroupName(),
                FormatString = displayFormatAttribute?.DataFormatString,
                NullDisplayText = displayFormatAttribute?.NullDisplayText,
                AutoGenerateField = displayAttribute?.GetAutoGenerateField() ?? true,
                VisibleAttributes = p.GetCustomAttributes<VisibleAttribute>(),
                DataType = dataTypeAttribute?.DataType,
                Styles = styleAttribute,
                IsEditable = editableFilter?.AllowEdit != false,
                EnabledAttributes = p.GetCustomAttributes<EnabledAttribute>(),
                SelectorConfiguration = selectorAttribute,
                UIHints = uiHintAttributes.Select(a => a.UIHint).ToArray(),
                IsDefaultLabelAllowed = p.PropertyType.UnwrapNullableType() != typeof(bool) // TODO: make this configurable, maybe?
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
