using System;
using System.Linq;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using Validator = DotVVM.Framework.Controls.Validator;

namespace DotVVM.AutoUI.Controls
{
    /// <summary> Renders a bulma table-like form: https://bulma.io/documentation/form/general/#horizontal-form </summary>
    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.InServerSideStyles)]
    public class BulmaForm : AutoFormBase
    {
        public BulmaForm(IServiceProvider services) : base(services)
        {
        }

        public DotvvmControl GetContents(FieldProps props)
        {
            var context = CreateAutoUiContext();

            var resultPlaceholder = new PlaceHolder();

            // create the rows
            foreach (var property in GetPropertiesToDisplay(context, props.FieldSelector))
            {
                if (TryGetFieldTemplate(property, props) is { } template)
                {
                    resultPlaceholder.AppendChildren(template);
                    continue;
                }

                DotvvmControl control;
                if (props.EditorTemplate.TryGetValue(property.Name, out var editorTemplate))
                    control = new TemplateHost(editorTemplate);
                else
                {
                    control = InitializeEditor(props, property, context);
                }

                var help = property.Description is { } description
                    ? new HtmlGenericControl("div").AddCssClass("help").SetProperty(c => c.InnerText, description.ToBinding(context.BindingService)!)
                    : null;
                var validator = new Validator()
                    .AddCssClass("help is-danger")
                    .SetProperty(Validator.ShowErrorMessageTextProperty, true);

                var field = new HtmlGenericControl("div")
                    .AddCssClass("field")
                    .AppendChildren(control, validator, help);
                var fieldBody = new HtmlGenericControl("div")
                    .AddCssClass("field-body")
                    .AppendChildren(field);

                var label = InitializeControlLabel(property, context, props)
                    ?.AddCssClass("label");
                var fieldLabel = new HtmlGenericControl("div")
                    .AddCssClass("field-label is-normal")
                    .AppendChildren(label);

                // create the validator
                InitializeValidation(validator, fieldLabel, property, context);

                var wrapperField = new HtmlGenericControl("div")
                    .AddCssClass("field is-horizontal")
                    .AppendChildren(fieldLabel, fieldBody);

                SetFieldVisibility(wrapperField, property, props, context);
                resultPlaceholder.Children.Add(wrapperField);
            }
            return resultPlaceholder;
        }

        private HtmlGenericControl InitializeEditor(FieldProps props, PropertyDisplayMetadata property, AutoUIContext context)
        {
            var editor = CreateEditor(property, context, props)
                .SetProperty(Validator.InvalidCssClassProperty, "is-danger")
                .SetProperty(Validator.SetToolTipTextProperty, true)
                .SetProperty(Validator.ValueProperty, context.CreateValueBinding(property));

            return new HtmlGenericControl("div")
                .AddCssClass("control")
                .AppendChildren(editor);
        }


        /// <summary>
        /// Indicates that when the AutoEditor control is used inside BulmaForm, it should be wrapped in a div with a 'select' CSS class. This attached property is intended to be used when implementing custom FormEditorProviders.
        /// </summary>
        [AttachedProperty(typeof(bool))]
        public static readonly DotvvmProperty WrapWithSelectClassProperty =
            DotvvmProperty.Register<bool, BulmaForm>(() => WrapWithSelectClassProperty, isValueInherited: false, defaultValue: false);

        /// <summary>
        /// Indicates that when the AutoEditor control is used inside BulmaForm, it should be wrapped in a div with a 'checkbox' CSS class. This attached property is intended to be used when implementing custom FormEditorProviders.
        /// </summary>
        [AttachedProperty(typeof(bool))]
        public static readonly DotvvmProperty WrapWithCheckboxClassProperty =
            DotvvmProperty.Register<bool, BulmaForm>(() => WrapWithCheckboxClassProperty, isValueInherited: false, defaultValue: false);

        /// <summary>
        /// Indicates that when the AutoEditor control is used inside BulmaForm, it should be wrapped in a div with a 'radio' CSS class. This attached property is intended to be used when implementing custom FormEditorProviders.
        /// </summary>
        [AttachedProperty(typeof(bool))]
        public static readonly DotvvmProperty WrapWithRadioClassProperty =
            DotvvmProperty.Register<bool, BulmaForm>(() => WrapWithRadioClassProperty, isValueInherited: false, defaultValue: false);

        /// <summary>
        /// Indicates that when the AutoEditor control is used inside BulmaForm, it should be wrapped in a div with a 'textarea' CSS class. This attached property is intended to be used when implementing custom FormEditorProviders.
        /// </summary>
        [AttachedProperty(typeof(bool))]
        public static readonly DotvvmProperty WrapWithTextareaClassProperty =
            DotvvmProperty.Register<bool, BulmaForm>(() => WrapWithTextareaClassProperty, isValueInherited: false, defaultValue: false);

        /// <summary>
        /// Indicates that when the AutoEditor control is used inside BulmaForm, it should be wrapped in a div with an 'input' CSS class. This attached property is intended to be used when implementing custom FormEditorProviders.
        /// </summary>
        [AttachedProperty(typeof(bool))]
        public static readonly DotvvmProperty WrapWithInputClassProperty =
            DotvvmProperty.Register<bool, BulmaForm>(() => WrapWithInputClassProperty, isValueInherited: false, defaultValue: false);

    }
}
