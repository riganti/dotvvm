using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
            if (!property.IsEditable)
            {
                var literal = new Literal();
                literal.SetBinding(Literal.TextProperty, props.Property);

                return literal;
            }


            var propertyType = property.PropertyInfo.PropertyType;
            var hasFormatstring = !string.IsNullOrEmpty(property.FormatString);
            var textBox = new TextBox()
                .SetCapability(props.Html)
                .AddCssClasses(ControlCssClass, property.Styles?.FormControlCssClass)
                .SetProperty(t => t.Text, props.Property)
                .SetAttribute("placeholder", property.Placeholder?.ToBinding(context))
                .SetAttribute("title", property.Description?.ToBinding(context))
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
                _ => propertyType.UnwrapNullableType() == typeof(DateTime) && !hasFormatstring ? TextBoxType.DateTimeLocal :
                     ReflectionUtils.IsNumericType(propertyType.UnwrapNullableType()) && !hasFormatstring ? TextBoxType.Number :
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

            var validators = context.ValidationMetadataProvider.GetAttributesForProperty(property.PropertyInfo).ToArray();
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
