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
        /// Gets or sets whether the control should be hidden for valid values.
        /// </summary>
        public bool HideWhenValid
        {
            get { return (bool)GetValue(HideWhenValidProperty); }
            set { SetValue(HideWhenValidProperty, value); }
        }
        public static readonly DotvvmProperty HideWhenValidProperty =
            DotvvmProperty.Register<bool, ValidationMessage>(c => c.HideWhenValid, true);

        /// <summary>
        /// Gets or sets a static text that will be displayed when the error occurs.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string StaticText
        {
            get { return (string)GetValue(StaticTextProperty); }
            set { SetValue(StaticTextProperty, value); }
        }
        public static readonly DotvvmProperty StaticTextProperty =
            DotvvmProperty.Register<string, ValidationMessage>(c => c.StaticText, "");

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
        /// Gets or sets whether the title attribute should be set to the error message.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool SetToolTipText
        {
            get { return (bool) GetValue(SetToolTipTextProperty); }
            set { SetValue(SetToolTipTextProperty, value);}
        }
        public static readonly DotvvmProperty SetToolTipTextProperty =
            DotvvmProperty.Register<bool, ValidationMessage>(c => c.SetToolTipText, true);

        /// <summary>
        /// Gets or sets whether the error message text should be displayed.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool ShowErrorMessageText
        {
            get { return (bool)GetValue(ShowErrorMessageTextProperty); }
            set { SetValue(ShowErrorMessageTextProperty, value); }
        }
        public static readonly DotvvmProperty ShowErrorMessageTextProperty =
            DotvvmProperty.Register<bool, ValidationMessage>(c => c.ShowErrorMessageText, false);


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
                writer.AddKnockoutDataBind("dotvvmValidation", this, ValidatedValueProperty);

                var group = GetValidationOptionsBindingGroup(writer, context);
                writer.AddKnockoutDataBind("dotvvmValidationOptions", group);
            }

            base.AddAttributesToRender(writer, context);
        }

        protected virtual KnockoutBindingGroup GetValidationOptionsBindingGroup(IHtmlWriter writer, RenderContext context)
        {
            var bindingGroup = new KnockoutBindingGroup();

            if (HideWhenValid)
            {
                bindingGroup.Add("hideWhenValid", "true");
            }

            if (!string.IsNullOrEmpty(InvalidCssClass))
            {
                bindingGroup.Add("invalidCssClass", KnockoutHelper.MakeStringLiteral(InvalidCssClass));
            }

            if (SetToolTipText)
            {
                bindingGroup.Add("setToolTipText", "true");
            }

            if (ShowErrorMessageText)
            {
                bindingGroup.Add("showErrorMessageText", "true");
            }

            return bindingGroup;
        }


        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            if (!string.IsNullOrEmpty(StaticText))
            {
                // render static value of the text property
                writer.WriteText(StaticText);
            }
            else
            {
                // render control contents
                RenderChildren(writer, context);
            }
        }
    }
}
