using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    /// <summary>
    /// Renders a TextBox control for properties of string, numeric or date type.
    /// </summary>
    public class TextBoxEditorProvider : FormEditorProviderBase
    {
        public override bool CanValidate => true;

        public override bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context)
        {
            return TextBoxHelper.CanHandleProperty(propertyInfo, context);
        }

        public override DotvvmControl CreateControl(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            if (!property.IsEditAllowed)
            {
                var literal = new Literal();
                literal.SetBinding(Literal.TextProperty, context.CreateValueBinding(property.PropertyInfo.Name));

                return literal;
            }

            var textBox = new TextBox();

            var cssClass = ControlHelpers.ConcatCssClasses(ControlCssClass, property.Styles?.FormControlCssClass);
            if (!string.IsNullOrEmpty(cssClass))
            {
                textBox.Attributes.Set("class", cssClass);
            }

            textBox.FormatString = property.FormatString;
            textBox.SetBinding(TextBox.TextProperty, context.CreateValueBinding(property.PropertyInfo.Name));

            if (property.DataType == DataType.Password)
            {
                textBox.Type = TextBoxType.Password;
            }
            else if (property.DataType == DataType.MultilineText)
            {
                textBox.Type = TextBoxType.MultiLine;
            }

            return textBox;
        }
    }
}
