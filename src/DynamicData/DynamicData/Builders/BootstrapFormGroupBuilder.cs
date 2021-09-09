using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors;

namespace DotVVM.Framework.Controls.DynamicData.Builders
{
    public class BootstrapFormGroupBuilder : FormBuilderBase
    {

        public string FormGroupCssClass { get; set; } = "form-group";

        public string LabelCssClass { get; set; } = "control-label";
        

        public override void BuildForm(DotvvmControl hostControl, DynamicDataContext dynamicDataContext)
        {
            var entityPropertyListProvider = dynamicDataContext.RequestContext.Configuration.ServiceProvider.GetService<IEntityPropertyListProvider>();
            
            // create the rows
            var properties = GetPropertiesToDisplay(dynamicDataContext, entityPropertyListProvider);
            foreach (var property in properties)
            {
                // find the editorProvider for cell
                var editorProvider = FindEditorProvider(property, dynamicDataContext);
                if (editorProvider == null) continue;

                // create the row
                HtmlGenericControl labelElement, controlElement;
                var formGroup = InitializeFormGroup(hostControl, property, dynamicDataContext, out labelElement, out controlElement);

                // create the label
                InitializeControlLabel(formGroup, labelElement, editorProvider, property, dynamicDataContext);

                // create the editorProvider
                InitializeControlEditor(formGroup, controlElement, editorProvider, property, dynamicDataContext);

                // create the validator
                InitializeValidation(formGroup, labelElement, controlElement, editorProvider, property, dynamicDataContext);
            }
        }

        

        protected virtual HtmlGenericControl InitializeFormGroup(DotvvmControl hostControl, PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext, out HtmlGenericControl labelElement, out HtmlGenericControl controlElement)
        {
            var formGroup = new HtmlGenericControl("div");
            formGroup.Attributes["class"] = ControlHelpers.ConcatCssClasses(FormGroupCssClass, property.Styles?.FormRowCssClass);
            hostControl.Children.Add(formGroup);

            labelElement = new HtmlGenericControl("label");
            labelElement.Attributes["class"] = LabelCssClass;
            formGroup.Children.Add(labelElement);

            controlElement = new HtmlGenericControl("div");
            controlElement.Attributes["class"] = ControlHelpers.ConcatCssClasses(property.Styles?.FormControlContainerCssClass);
            formGroup.Children.Add(controlElement);

            return formGroup;
        }

        protected virtual void InitializeControlLabel(HtmlGenericControl formGroup, HtmlGenericControl labelElement, IFormEditorProvider editorProvider, PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext)
        {
            if (editorProvider.RenderDefaultLabel)
            {
                labelElement.Children.Add(new Literal(property.DisplayName));
            }
        }

        protected virtual void InitializeControlEditor(HtmlGenericControl formGroup, HtmlGenericControl controlElement, IFormEditorProvider editorProvider, PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext)
        {
            editorProvider.CreateControl(controlElement, property, dynamicDataContext);

            // set CSS classes
            foreach (var control in controlElement.GetAllDescendants())
            {
                if (control is TextBox || control is ComboBox)
                {
                    ((HtmlGenericControl) control).Attributes["class"] = "form-control";
                }
            }
        }

        protected virtual void InitializeValidation(HtmlGenericControl formGroup, HtmlGenericControl labelElement, HtmlGenericControl controlElement, IFormEditorProvider editorProvider, PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext)
        {
            if (dynamicDataContext.ValidationMetadataProvider.GetAttributesForProperty(property.PropertyInfo).OfType<RequiredAttribute>().Any())
            {
                labelElement.Attributes["class"] = ControlHelpers.ConcatCssClasses(labelElement.Attributes["class"] as string, "dynamicdata-required");
            }

            if (editorProvider.CanValidate)
            {
                controlElement.SetValue(Validator.ValueProperty, editorProvider.GetValidationValueBinding(property, dynamicDataContext));
            }
        }

    }
}
