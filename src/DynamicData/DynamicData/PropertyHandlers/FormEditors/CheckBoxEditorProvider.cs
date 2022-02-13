using System;
using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    /// <summary>
    /// Renders a CheckBox control for properties of boolean type.
    /// </summary>
    public class CheckBoxEditorProvider : FormEditorProviderBase
    {
        public override bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context)
        {
            return ReflectionUtils.UnwrapNullableType(propertyInfo.PropertyType) == typeof(bool);
        }
        
        public override bool CanValidate => true;

        public override bool RenderDefaultLabel => false;

        public override DotvvmControl CreateControl(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            var checkBox = new CheckBox();

            var cssClass = ControlHelpers.ConcatCssClasses(ControlCssClass, property.Styles?.FormControlCssClass);
            if (!string.IsNullOrEmpty(cssClass))
            {
                checkBox.Attributes.Set("class", cssClass);
            }

            checkBox.Text = property.DisplayName ?? "";
            checkBox.SetBinding(CheckBox.CheckedProperty, context.CreateValueBinding(property.PropertyInfo.Name));

            return checkBox;
        }
    }
}
