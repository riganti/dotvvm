using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Controls;

namespace DotVVM.AutoUI.PropertyHandlers.FormEditors
{
    public abstract class FormEditorProviderBase : DynamicDataPropertyHandlerBase, IFormEditorProvider
    {
        public string? ControlCssClass { get; set; }

        public abstract DotvvmControl CreateControl(PropertyDisplayMetadata property, DynamicEditor.Props props, DynamicDataContext context);
    }
}
