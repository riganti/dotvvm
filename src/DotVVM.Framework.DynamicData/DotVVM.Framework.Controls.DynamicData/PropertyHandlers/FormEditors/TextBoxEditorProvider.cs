using System;
using System.Collections.Generic;
using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    public class TextBoxEditorProvider : FormEditorProviderBase
    {


        public override bool CanValidate => true;

        public override bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context)
        {
            return TextBoxHelper.CanHandleProperty(propertyInfo, context);
        }

        public override void CreateControl(DotvvmControl container, PropertyDisplayMetadata property, DynamicDataContext context)
        {
            var textBox = new TextBox();
            container.Children.Add(textBox);

            if (!string.IsNullOrEmpty(ControlCssClass))
            {
                textBox.Attributes["class"] = ControlCssClass;
            }

            textBox.ValueType = TextBoxHelper.GetValueType(property.PropertyInfo);
            textBox.FormatString = property.FormatString;
            textBox.SetBinding(TextBox.TextProperty, context.CreateValueBinding(property.PropertyInfo.Name));
        }
        
        
    }
}