using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    public interface IFormEditorProvider : IDynamicDataPropertyHandler
    {
        DotvvmControl CreateControl(PropertyDisplayMetadata property, DynamicEditor.Props props, DynamicDataContext context);
    }
}
