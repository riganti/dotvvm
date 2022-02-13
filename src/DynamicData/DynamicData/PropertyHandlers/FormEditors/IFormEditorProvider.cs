using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    public interface IFormEditorProvider : IDynamicDataPropertyHandler
    {

        bool RenderDefaultLabel { get; }

        bool CanValidate { get; }

        ValueBindingExpression GetValidationValueBinding(PropertyDisplayMetadata property, DynamicDataContext context);

        DotvvmControl CreateControl(PropertyDisplayMetadata property, DynamicDataContext context);

    }
}
