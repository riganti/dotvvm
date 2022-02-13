using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls.DynamicData.Metadata;

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

            var textBox = new TextBox()
                .AddCssClasses(ControlCssClass, property.Styles?.FormControlCssClass)
                .SetProperty(t => t.Text, props.Property)
                .SetProperty(t => t.FormatString, property.FormatString)
                .SetProperty(t => t.Enabled, props.Enabled)
                .SetProperty(t => t.Changed, props.Changed)
                .SetCapability(props.Html);

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
