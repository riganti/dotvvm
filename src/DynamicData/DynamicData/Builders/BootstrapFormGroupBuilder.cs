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

                // create the row
                HtmlGenericControl labelElement, controlElement;
                var formGroup = InitializeFormGroup(property, dynamicDataContext, out labelElement, out controlElement);

                // create the label
                labelElement.AppendChildren(InitializeControlLabel(property, dynamicDataContext));

                // create the editorProvider
                controlElement.AppendChildren(InitializeControlEditor(editorProvider, property, dynamicDataContext));

                // create the validator
                InitializeValidation(formGroup, labelElement, controlElement, editorProvider, property, dynamicDataContext);

                resultPlaceholder.Children.Add(formGroup);
            }
            return resultPlaceholder;
        }



        protected virtual HtmlGenericControl InitializeFormGroup(PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext, out HtmlGenericControl labelElement, out HtmlGenericControl controlElement)
        {
            var formGroup = new HtmlGenericControl("div");
            formGroup.AddCssClass(FormGroupCssClass).AddCssClass(property.Styles?.FormRowCssClass);

            labelElement = new HtmlGenericControl("label");
            labelElement.Attributes.Add("class", LabelCssClass);
            formGroup.Children.Add(labelElement);

            controlElement = new HtmlGenericControl("div");
            controlElement.Attributes.Add("class", property.Styles?.FormControlContainerCssClass?.Trim());
            formGroup.Children.Add(controlElement);

            return formGroup;
        }

        protected virtual DotvvmControl? InitializeControlLabel(PropertyDisplayMetadata property, DynamicDataContext ddContext)
        {
            if (property.IsDefaultLabelAllowed)
                return new Literal(property.DisplayName);
            return null;
        }

        protected virtual DotvvmControl InitializeControlEditor(IFormEditorProvider editorProvider, PropertyDisplayMetadata property, DynamicDataContext ddContext)
        {
            var editor =
                new DynamicEditor(ddContext.Services)
                    .SetProperty(DynamicEditor.PropertyProperty, ddContext.CreateValueBinding(property.PropertyInfo.Name));
            editor.AddAttribute("class", "form-control");
            return editor;
        }

        protected virtual void InitializeValidation(HtmlGenericControl formGroup, HtmlGenericControl labelElement, HtmlGenericControl controlElement, IFormEditorProvider editorProvider, PropertyDisplayMetadata property, DynamicDataContext ddContext)
        {
            if (ddContext.ValidationMetadataProvider.GetAttributesForProperty(property.PropertyInfo).OfType<RequiredAttribute>().Any())
            {
                labelElement.Attributes.Set("class", ControlHelpers.ConcatCssClasses(labelElement.Attributes["class"] as string, "dynamicdata-required"));
            }

            controlElement.SetValue(Validator.ValueProperty, ddContext.CreateValueBinding(property.PropertyInfo.Name));
        }

    }
}
