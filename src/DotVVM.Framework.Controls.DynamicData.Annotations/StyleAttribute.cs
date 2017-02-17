using System;

namespace DotVVM.Framework.Controls.DynamicData.Annotations
{
    /// <summary>
    /// Defines the CSS classes applied to the field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class StyleAttribute : Attribute
    {

        /// <summary>
        /// Gets or sets the CSS class applied to the container of the form control (e.g. table cell which contains the TextBox control) for this field.
        /// </summary>
        public string FormControlContainerCssClass { get; set; }

        /// <summary>
        /// Gets or sets the CSS class applied to the row in the form (e.g. table row which contains the label and the TextBox control) for this field.
        /// </summary>
        public string FormRowCssClass { get; set; }

        /// <summary>
        /// Gets or sets the CSS class applied to the control in the form.
        /// </summary>
        public string FormControlCssClass { get; set; }

        /// <summary>
        /// Gets or sets the CSS class applied to the GridView table cell for this field.
        /// </summary>
        public string GridCellCssClass { get; set; }

        /// <summary>
        /// Gets or sets the CSS class applied to the GridView table header cell for this field.
        /// </summary>
        public string GridHeaderCellCssClass { get; set; }
        
    }
}
