using System;
using System.Linq;
using DotVVM.Framework.Controls;
using Validator = DotVVM.Framework.Controls.Validator;

namespace DotVVM.AutoUI.Controls
{
    /// <summary> Renders a bulma table-like form: https://bulma.io/documentation/form/general/#horizontal-form </summary>
    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.InServerSideStyles)]
    public class BulmaDynamicEntity : DynamicEntityBase
    {
        public BulmaDynamicEntity(IServiceProvider services) : base(services)
        {
        }

        public DotvvmControl GetContents(FieldProps props)
        {
            var context = CreateDynamicDataContext();

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
                    var input = CreateEditor(property, context, props)
                        .AddCssClass("input")
                        .SetProperty(Validator.InvalidCssClassProperty, "is-danger")
                        .SetProperty(Validator.SetToolTipTextProperty, true)
                        .SetProperty(Validator.ValueProperty, context.CreateValueBinding(property));
                    input.Tags = input.Tags.Append("bulma").ToArray();
                    control = new HtmlGenericControl("div")
                        .AddCssClass("control")
                        .AppendChildren(input);
                }

                var help = property.Description is { } description
                    ? new HtmlGenericControl("div").AddCssClass("help").SetProperty(c => c.InnerText, description.ToBinding(context)!)
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

    }
}
