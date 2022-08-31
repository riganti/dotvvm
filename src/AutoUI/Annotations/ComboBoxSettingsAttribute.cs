using System;

namespace DotVVM.AutoUI.Annotations
{
    /// <summary>
    /// Defines the settings for the ComboBox form editor provider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ComboBoxSettingsAttribute : Attribute
    {

        /// <summary>
        /// Gets or sets the name of the property to be displayed.
        /// </summary>
        public string DisplayMember { get; set; }

        /// <summary>
        /// Gets or sets the name of the property to be used as selected value.
        /// </summary>
        public string ValueMember { get; set; }

        /// <summary>
        /// Gets or sets the binding expression for the list of items.
        /// </summary>
        public string DataSourceBinding { get; set; }

        /// <summary>
        /// Gets or sets the text on the empty item. If null or empty, the empty item will not be included.
        /// </summary>
        public string EmptyItemText { get; set; }

    }
}
