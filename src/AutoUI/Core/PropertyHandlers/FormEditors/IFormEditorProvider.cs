using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Controls;

namespace DotVVM.AutoUI.PropertyHandlers.FormEditors
{
    public interface IFormEditorProvider : IDynamicDataPropertyHandler
    {
        DotvvmControl CreateControl(PropertyDisplayMetadata property, AutoEditor.Props props, DynamicDataContext context);
    }
}
