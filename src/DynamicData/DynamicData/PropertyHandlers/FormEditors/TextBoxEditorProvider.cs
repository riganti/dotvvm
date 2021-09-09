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

        public override void CreateControl(DotvvmControl container, PropertyDisplayMetadata property, DynamicDataContext context)
        {
            if (!property.IsEditAllowed)
            {
                var literal = new Literal();
                container.Children.Add(literal);
                literal.SetBinding(Literal.TextProperty, context.CreateValueBinding(property.PropertyInfo.Name));

                return;
            }

            var textBox = new TextBox();
            container.Children.Add(textBox);

            var cssClass = ControlHelpers.ConcatCssClasses(ControlCssClass, property.Styles?.FormControlCssClass);
            if (!string.IsNullOrEmpty(cssClass))
            {
                textBox.Attributes["class"] = cssClass;
            }

            textBox.ValueType = TextBoxHelper.GetValueTypeOrDefault(property.PropertyInfo);
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

            if (textBox.IsPropertySet(DynamicEntity.EnabledProperty))
            {
                ControlHelpers.CopyProperty(textBox, DynamicEntity.EnabledProperty, textBox, TextBox.EnabledProperty);
            }
        }
    }
}