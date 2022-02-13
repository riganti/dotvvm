using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using DotVVM.Framework.Controls.DynamicData.Annotations;

namespace DotVVM.Framework.Controls.DynamicData.Metadata
{
    /// <summary>
    /// Provides information about how the property is displayed in the user interface.
    /// </summary>
    public class DataAnnotationsPropertyDisplayMetadataProvider : IPropertyDisplayMetadataProvider
    {

        private readonly ConcurrentDictionary<PropertyCulturePair, PropertyDisplayMetadata> cache = new ConcurrentDictionary<PropertyCulturePair, PropertyDisplayMetadata>();

        /// <summary>
        /// Get the metadata about how the property is displayed.
        /// </summary>
        public PropertyDisplayMetadata GetPropertyMetadata(PropertyInfo property)
        {
            return cache.GetOrAdd(new PropertyCulturePair(property, CultureInfo.CurrentUICulture), GetPropertyMetadataCore);
        }

        private PropertyDisplayMetadata GetPropertyMetadataCore(PropertyCulturePair pair)
        {
            var displayAttribute = pair.PropertyInfo.GetCustomAttribute<DisplayAttribute>();
            var displayFormatAttribute = pair.PropertyInfo.GetCustomAttribute<DisplayFormatAttribute>();
            var dataTypeAttribute = pair.PropertyInfo.GetCustomAttribute<DataTypeAttribute>();
            var styleAttribute = pair.PropertyInfo.GetCustomAttribute<StyleAttribute>();
            var visibilityFilters = pair.PropertyInfo.GetCustomAttributes().OfType<IVisibilityFilter>().ToArray();
            var editableFilter = pair.PropertyInfo.GetCustomAttribute<EditableAttribute>();
            var selectorAttribute = pair.PropertyInfo.GetCustomAttribute<SelectorAttribute>();

            return new PropertyDisplayMetadata()
            {
                PropertyInfo = pair.PropertyInfo,
                DisplayName = displayAttribute?.GetName(),
                Order = displayAttribute?.GetOrder(),
                GroupName = displayAttribute?.GetGroupName(),
                FormatString = displayFormatAttribute?.DataFormatString,
                NullDisplayText = displayFormatAttribute?.NullDisplayText,
                AutoGenerateField = displayAttribute?.GetAutoGenerateField() ?? true,
                DataType = dataTypeAttribute?.DataType,
                VisibilityFilters = visibilityFilters,
                Styles = styleAttribute,
                IsEditAllowed = editableFilter?.AllowEdit != false,
                SelectorConfiguration = selectorAttribute
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
