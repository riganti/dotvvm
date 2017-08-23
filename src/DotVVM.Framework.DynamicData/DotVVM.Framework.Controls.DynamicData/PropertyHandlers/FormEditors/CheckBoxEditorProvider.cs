using System;
using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    public class CheckBoxEditorProvider : FormEditorProviderBase
    {
        public override bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context)
        {
            return UnwrapNullableType(propertyInfo.PropertyType) == typeof(bool);
        }
        
        public override bool CanValidate => true;

        public override bool RenderDefaultLabel => false;

        public override void CreateControl(DotvvmControl container, PropertyDisplayMetadata property, DynamicDataContext context)
        {
            var checkBox = new CheckBox();
            container.Children.Add(checkBox);

            if (!string.IsNullOrEmpty(ControlCssClass))
            {
                checkBox.Attributes["class"] = ControlCssClass;
            }

            checkBox.Text = property.DisplayName;
            checkBox.SetBinding(CheckableControlBase.CheckedValueProperty, context.CreateValueBinding(property.PropertyInfo.Name));
        }
    }
}