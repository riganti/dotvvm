using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Controls;

namespace DotVVM.AutoUI.PropertyHandlers.FormEditors
{
    public interface IFormEditorProvider : IAutoUIPropertyHandler
    {
        DotvvmControl CreateControl(PropertyDisplayMetadata property, AutoEditor.Props props, AutoUIContext context);
    }
}
