using System.Reflection;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors
{
    public abstract class FormEditorProviderBase : DynamicDataPropertyHandlerBase, IFormEditorProvider
    {
        public string ControlCssClass { get; set; }

        public virtual bool RenderDefaultLabel => true;

        public virtual bool CanValidate => false;

        public ValueBindingExpression GetValidationValueBinding(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            return context.CreateValueBinding(property.PropertyInfo.Name);
        }

        public abstract void CreateControl(DotvvmControl container, PropertyDisplayMetadata property, DynamicDataContext context);

        protected virtual void SetValidatorValueBinding(DotvvmBindableObject textBox, BindingExpression valueBindingExpression)
        {
            if (CanValidate)
            {
                textBox.SetBinding(Validator.ValueProperty, valueBindingExpression);
            }
        }
    }
}