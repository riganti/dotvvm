using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    public abstract class FormEditorProviderBase : DynamicDataPropertyHandlerBase, IFormEditorProvider
    {
        public string ControlCssClass { get; set; }

        public abstract DotvvmControl CreateControl(PropertyDisplayMetadata property, DynamicDataContext context);
    }
}
