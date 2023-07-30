using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.AutoUI.PropertyHandlers.FormEditors
{
    /// <summary>
    /// Renders a TextBox control for properties of string, numeric or date type.
    /// </summary>
    public class TextBoxEditorProvider : FormEditorProviderBase
    {
        public override bool CanHandleProperty(PropertyDisplayMetadata property, AutoUIContext context)
        {
            return TextBoxHelper.CanHandleProperty(property.Type);
        }

        public override DotvvmControl CreateControl(PropertyDisplayMetadata property, AutoEditor.Props props, AutoUIContext context)
        {
            if (!property.IsEditable)
            {
                var literal = new Literal();
                literal.SetBinding(Literal.TextProperty, props.Property);
                return literal;
            }


            var propertyType = property.Type;
            var hasFormatString = !string.IsNullOrEmpty(property.FormatString);
            var textBox = new TextBox()
                .SetCapability(props.Html)
                .AddCssClasses(ControlCssClass, property.Styles?.FormControlCssClass)
                .SetProperty(t => t.Text, props.Property)
                .SetAttribute("placeholder", property.Placeholder?.ToBinding(context.BindingService))
                .SetAttribute("title", property.Description?.ToBinding(context.BindingService))
                .SetProperty(t => t.FormatString, property.FormatString)
                .SetProperty(t => t.Enabled, props.Enabled)
                .SetProperty(t => t.Changed, props.Changed);

            textBox.Type = property.DataType switch {
                DataType.Password => TextBoxType.Password,
                DataType.MultilineText => TextBoxType.MultiLine,
                DataType.Date => TextBoxType.Date,
                DataType.Time => TextBoxType.Time,
                DataType.DateTime => TextBoxType.DateTimeLocal,
                DataType.EmailAddress => TextBoxType.Email,
                DataType.PhoneNumber => TextBoxType.Telephone,
                DataType.Url => TextBoxType.Url,
                DataType.ImageUrl => TextBoxType.Url,
                _ => hasFormatString ? TextBoxType.Normal :
                     propertyType.UnwrapNullableType() == typeof(DateTime) ? TextBoxType.DateTimeLocal :
                     propertyType.UnwrapNullableType() == typeof(DateOnly) ? TextBoxType.Date :
                     propertyType.UnwrapNullableType() == typeof(TimeOnly) ? TextBoxType.Time :
                     ReflectionUtils.IsNumericType(propertyType.UnwrapNullableType()) ? TextBoxType.Number :
                     TextBoxType.Normal
            };

            if (textBox.Type == TextBoxType.Number)
            {
                var type = propertyType.UnwrapNullableType();
                if (type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong))
                {
                    textBox.SetAttribute("min", "0");
                }
            }

            var validators = context.GetPropertyValidators(property);
            if (validators.OfType<RequiredAttribute>().Any())
            {
                textBox.SetAttribute("required", true);
            }

            if (validators.OfType<RegularExpressionAttribute>().FirstOrDefault() is {} pattern)
            {
                textBox.SetAttribute("pattern", pattern.Pattern);
            }

            if (validators.OfType<StringLengthAttribute>().FirstOrDefault() is {} stringLength)
            {
                if (stringLength.MinimumLength > 0)
                {
                    textBox.SetAttribute("minlength", stringLength.MinimumLength.ToString());
                }
                textBox.SetAttribute("maxlength", stringLength.MaximumLength.ToString());
            }
            else
            {
                if (validators.OfType<MaxLengthAttribute>().FirstOrDefault() is {} maxLength)
                {
                    textBox.SetAttribute("maxlength", maxLength.Length.ToString());
                }

                if (validators.OfType<MinLengthAttribute>().FirstOrDefault() is {} minLength)
                {
                    textBox.SetAttribute("minlength", minLength.Length.ToString());
                }
            }

            if (textBox.Type == TextBoxType.Number && validators.OfType<RangeAttribute>().FirstOrDefault() is {} range)
            {
                if (range.Minimum is {})
                    textBox.SetAttribute("min", range.Minimum.ToString());
                if (range.Maximum is {})
                    textBox.SetAttribute("max", range.Maximum.ToString());
            }

            return textBox;
        }
    }
}
