using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.AutoUI.PropertyHandlers.FormEditors
{
    /// <summary>
    /// Renders a CheckBox control for properties of boolean type.
    /// </summary>
    public class CheckBoxEditorProvider : FormEditorProviderBase
    {
        public override bool CanHandleProperty(PropertyDisplayMetadata property, AutoUIContext context)
        {
            return ReflectionUtils.UnwrapNullableType(property.Type) == typeof(bool);
        }
        
        public override DotvvmControl CreateControl(PropertyDisplayMetadata property, AutoEditor.Props props, AutoUIContext context)
        {
            var checkBox = new CheckBox()
                .AddCssClasses(ControlCssClass, property.Styles?.FormControlCssClass)
                .SetProperty(c => c.Changed, props.Changed)
                .SetProperty(c => c.Checked, props.Property)
                .SetProperty(c => c.Text, property.GetDisplayName().ToBinding(context))
                .SetProperty(c => c.Enabled, props.Enabled);
            return checkBox;
        }
    }
}
