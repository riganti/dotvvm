using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    /// <summary>
    /// Renders a TextBox control for properties of string, numeric or date type.
    /// </summary>
    public class TextBoxEditorProvider : FormEditorProviderBase
    {
        public override bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context)
        {
            return TextBoxHelper.CanHandleProperty(propertyInfo, context);
        }

        public override DotvvmControl CreateControl(PropertyDisplayMetadata property, DynamicEditor.Props props, DynamicDataContext context)
        {
            if (!property.IsEditAllowed)
            {
                var literal = new Literal();
                literal.SetBinding(Literal.TextProperty, props.Property);

                return literal;
            }

            var propertyType = property.PropertyInfo.PropertyType;
            var hasFormatstring = !string.IsNullOrEmpty(property.FormatString);
            var textBox = new TextBox()
                .AddCssClasses(ControlCssClass, property.Styles?.FormControlCssClass)
                .SetProperty(t => t.Text, props.Property)
                .SetProperty(t => t.FormatString, property.FormatString)
                .SetProperty(t => t.Enabled, props.Enabled)
                .SetProperty(t => t.Changed, props.Changed)
                .SetCapability(props.Html);

            textBox.Type = property.DataType switch {
                DataType.Password => TextBoxType.Password,
                DataType.MultilineText => TextBoxType.MultiLine,
                DataType.Date => TextBoxType.Date,
                DataType.Time => TextBoxType.Time,
                DataType.DateTime => TextBoxType.DateTimeLocal,
                DataType.EmailAddress => TextBoxType.Email,
                DataType.PhoneNumber => TextBoxType.Telephone,
                _ => propertyType.UnwrapNullableType() == typeof(DateTime) && !hasFormatstring ? TextBoxType.DateTimeLocal :
                     ReflectionUtils.IsNumericType(propertyType.UnwrapNullableType()) && !hasFormatstring ? TextBoxType.Number :
                     TextBoxType.Normal
            };

            return textBox;
        }
    }
}
