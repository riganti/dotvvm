using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Displays the asterisk or validation message for a specified field.
    /// </summary>
    public class ValidationMessage : HtmlGenericControl 
    {

        /// <summary>
        /// Gets or sets the text that will be displayed when the error occurs. By default the control shows the asterisk character.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string AsteriskText
        {
            get { return (string)GetValue(AsteriskTextProperty); }
            set { SetValue(AsteriskTextProperty, value); }
        }
        public static readonly DotvvmProperty AsteriskTextProperty =
            DotvvmProperty.Register<string, ValidationMessage>(c => c.AsteriskText, "*");


        /// <summary>
        /// Gets or sets the mode how the validator appears.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public ValidationMessageMode Mode
        {
            get { return (ValidationMessageMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }
        public static readonly DotvvmProperty ModeProperty =
            DotvvmProperty.Register<ValidationMessageMode, ValidationMessage>(c => c.Mode, ValidationMessageMode.HideWhenValid);


        /// <summary>
        /// Gets or sets a binding that points to the validated value.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public object ValidatedValue
        {
            get { return (object)GetValue(ValidatedValueProperty); }
            set { SetValue(ValidatedValueProperty, value); }
        }
        public static readonly DotvvmProperty ValidatedValueProperty =
            DotvvmProperty.Register<object, ValidationMessage>(c => c.ValidatedValue, null);


        /// <summary>
        /// Gets or sets the name of CSS class which is applied to the control when it is not valid.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string InvalidCssClass
        {
            get { return (string)GetValue(InvalidCssClassProperty); }
            set { SetValue(InvalidCssClassProperty, value); }
        }
        public static readonly DotvvmProperty InvalidCssClassProperty =
            DotvvmProperty.Register<string, ValidationMessage>(c => c.InvalidCssClass, "field-validation-error");



        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationMessage"/> class.
        /// </summary>
        public ValidationMessage()
        {
            TagName = "span";
        }


        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            var validatedValueBinding = GetValueBinding(ValidatedValueProperty);
            if (validatedValueBinding != null)
            {
                writer.AddKnockoutDataBind("dotvvmalidation", this, ValidatedValueProperty, () => { });
                
                var options = string.Format("{{'mode':'{0}', 'cssClass':{1}}}", KnockoutHelper.ConvertToCamelCase(Mode.ToString()), KnockoutHelper.MakeStringLiteral(InvalidCssClass));
                writer.AddKnockoutDataBind("dotvvmalidationOptions", options);
            }

            base.AddAttributesToRender(writer, context);
        }


        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            var textBinding = GetBinding(AsteriskTextProperty);
            if (textBinding == null && !string.IsNullOrEmpty(AsteriskText))
            {
                // render static value of the text property
                writer.WriteText(AsteriskText);
            }
            else
            {
                // render control contents
                RenderChildren(writer, context);
            }
        }
    }
}
