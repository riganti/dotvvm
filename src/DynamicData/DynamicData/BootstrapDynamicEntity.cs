using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData
{
    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.Always)]
    public class BootstrapDynamicEntity : DynamicEntityBase
    {
        public BootstrapDynamicEntity(IServiceProvider services) : base(services)
        {
        }

        public string? LabelCssClass
        {
            get { return (string?)GetValue(LabelCssClassProperty); }
            set { SetValue(LabelCssClassProperty, value); }
        }
        public static readonly DotvvmProperty LabelCssClassProperty =
            DotvvmProperty.Register<string, BootstrapDynamicEntity>(nameof(LabelCssClass), "control-label");

        public string? FormGroupCssClass
        {
            get { return (string?)GetValue(FormGroupCssClassProperty); }
            set { SetValue(FormGroupCssClassProperty, value); }
        }
        public static readonly DotvvmProperty FormGroupCssClassProperty =
            DotvvmProperty.Register<string, BootstrapDynamicEntity>(nameof(FormGroupCssClass), "form-group");

        public DotvvmControl GetContents(FieldProps props)
        {
            var context = this.CreateDynamicDataContext();

            var resultPlaceholder = new PlaceHolder();

            // create the rows
            foreach (var property in GetPropertiesToDisplay(context))
            {
                // create the row
                HtmlGenericControl labelElement, controlElement;
                var formGroup = InitializeFormGroup(property, context, out labelElement, out controlElement);

                // create the label
                labelElement.AppendChildren(InitializeControlLabel(property, context));

                // create the editorProvider
                controlElement.AppendChildren(
                    this.CreateEditor(property, context, props)
                        .AddCssClass("form-control")
                );

                // create the validator
                InitializeValidation(controlElement, labelElement, property, context);

                resultPlaceholder.Children.Add(formGroup);
            }
            return resultPlaceholder;
        }

        protected virtual HtmlGenericControl InitializeFormGroup(PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext, out HtmlGenericControl labelElement, out HtmlGenericControl controlElement)
        {

            labelElement = new HtmlGenericControl("label")
                .AddCssClass(LabelCssClass);

            controlElement = new HtmlGenericControl("div")
                .AddCssClass(property.Styles?.FormControlContainerCssClass);

            return
                new HtmlGenericControl("div")
                .AddCssClasses(FormGroupCssClass, property.Styles?.FormRowCssClass)
                .SetProperty(c => c.IncludeInPage, GetVisibleResourceBinding(property, dynamicDataContext))
                .AppendChildren(labelElement, controlElement);
        }

        protected virtual DotvvmControl? InitializeControlLabel(PropertyDisplayMetadata property, DynamicDataContext ddContext)
        {
            if (property.IsDefaultLabelAllowed)
                return new Literal(property.DisplayName?.ToBinding(ddContext.BindingService) ?? new ValueOrBinding<string>(""));
            return null;
        }

    }
}
