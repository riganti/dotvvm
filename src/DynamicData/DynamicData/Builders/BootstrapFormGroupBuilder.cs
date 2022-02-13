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


        public override DotvvmControl BuildForm(DynamicDataContext dynamicDataContext)
        {
            var entityPropertyListProvider = dynamicDataContext.Services.GetService<IEntityPropertyListProvider>();

            var resultPlaceholder = new PlaceHolder();

            // create the rows
            var properties = GetPropertiesToDisplay(dynamicDataContext, entityPropertyListProvider);
            foreach (var property in properties)
            {
                // find the editorProvider for cell
                var editorProvider = FindEditorProvider(property, dynamicDataContext);
                if (editorProvider == null) continue;

                // create the row
                HtmlGenericControl labelElement, controlElement;
                var formGroup = InitializeFormGroup(property, dynamicDataContext, out labelElement, out controlElement);

                // create the label
                InitializeControlLabel(formGroup, labelElement, editorProvider, property, dynamicDataContext);

                // create the editorProvider
                InitializeControlEditor(formGroup, controlElement, editorProvider, property, dynamicDataContext);

                // create the validator
                InitializeValidation(formGroup, labelElement, controlElement, editorProvider, property, dynamicDataContext);

                resultPlaceholder.Children.Add(formGroup);
            }
            return resultPlaceholder;
        }



        protected virtual HtmlGenericControl InitializeFormGroup(PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext, out HtmlGenericControl labelElement, out HtmlGenericControl controlElement)
        {
            var formGroup = new HtmlGenericControl("div");
            formGroup.Attributes.Set("class", ControlHelpers.ConcatCssClasses(FormGroupCssClass, property.Styles?.FormRowCssClass));

            labelElement = new HtmlGenericControl("label");
            labelElement.Attributes.Set("class", LabelCssClass);
            formGroup.Children.Add(labelElement);

            controlElement = new HtmlGenericControl("div");
            controlElement.Attributes.Set("class", ControlHelpers.ConcatCssClasses(property.Styles?.FormControlContainerCssClass));
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
            controlElement.AppendChildren(editorProvider.CreateControl(property, dynamicDataContext));

            // set CSS classes
            foreach (var control in controlElement.GetAllDescendants())
            {
                if (control is TextBox || control is ComboBox)
                {
                    ((HtmlGenericControl) control).Attributes.Set("class", "form-control");
                }
            }
        }

        protected virtual void InitializeValidation(HtmlGenericControl formGroup, HtmlGenericControl labelElement, HtmlGenericControl controlElement, IFormEditorProvider editorProvider, PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext)
        {
            if (dynamicDataContext.ValidationMetadataProvider.GetAttributesForProperty(property.PropertyInfo).OfType<RequiredAttribute>().Any())
            {
                labelElement.Attributes.Set("class", ControlHelpers.ConcatCssClasses(labelElement.Attributes["class"] as string, "dynamicdata-required"));
            }

            if (editorProvider.CanValidate)
            {
                controlElement.SetValue(Validator.ValueProperty, editorProvider.GetValidationValueBinding(property, dynamicDataContext));
            }
        }

    }
}
