using System;
using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    /// <summary>
    /// Renders a CheckBox control for properties of boolean type.
    /// </summary>
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

            var cssClass = ControlHelpers.ConcatCssClasses(ControlCssClass, property.Styles?.FormControlCssClass);
            if (!string.IsNullOrEmpty(cssClass))
            {
                checkBox.Attributes.Set("class", cssClass);
            }

            checkBox.Text = property.DisplayName;
            checkBox.SetBinding(CheckBox.CheckedProperty, context.CreateValueBinding(property.PropertyInfo.Name));

            if (checkBox.IsPropertySet(DynamicEntity.EnabledProperty))
            {
                ControlHelpers.CopyProperty(checkBox, DynamicEntity.EnabledProperty, checkBox, CheckableControlBase.EnabledProperty);
            }
        }
    }
}
