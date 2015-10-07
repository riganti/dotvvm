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
        /// Gets or sets whether the control should be hidden even for valid values.
        /// </summary>
        [AttachedProperty(typeof(bool))]
        public static readonly ActiveDotvvmProperty HideWhenValidProperty =
            DelegateActionProperty<bool>.Register<ValidationMessage>("HideWhenValid", AddHideWhenValid);

        private static void AddHideWhenValid(IHtmlWriter writer, RenderContext context, bool value, DotvvmControl control)
        {
            if (value)
            {
                var bindingGroup = new KnockoutBindingGroup();
                bindingGroup.Add("hideWhenValid", "true");
                writer.AddKnockoutDataBind("dotvvmValidationOptions", bindingGroup);
            }
        }

        /// <summary>
        /// Gets or sets the name of CSS class which is applied to the control when it is not valid.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        [AttachedProperty(typeof(string))]
        public static readonly ActiveDotvvmProperty InvalidCssClassProperty =
            DelegateActionProperty<string>.Register<ValidationMessage>("InvalidCssClass", AddInvalidCssClass);

        private static void AddInvalidCssClass(IHtmlWriter writer, RenderContext context, string value, DotvvmControl control)
        {
            var bindingGroup = new KnockoutBindingGroup();
            bindingGroup.Add("invalidCssClass", KnockoutHelper.MakeStringLiteral(value));
            writer.AddKnockoutDataBind("dotvvmValidationOptions", bindingGroup);
        }


        /// <summary>
        /// Gets or sets whether the title attribute should be set to the error message.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        [AttachedProperty(typeof(bool))]
        public static readonly ActiveDotvvmProperty SetToolTipTextProperty =
            DelegateActionProperty<bool>.Register<ValidationMessage>("SetToolTipText", AddSetToolTipText);

        private static void AddSetToolTipText(IHtmlWriter writer, RenderContext context, bool value, DotvvmControl control)
        {
            if (value)
            {
                var bindingGroup = new KnockoutBindingGroup();
                bindingGroup.Add("setToolTipText", "true");
                writer.AddKnockoutDataBind("dotvvmValidationOptions", bindingGroup);
            }
        }


        /// <summary>
        /// Gets or sets whether the error message text should be displayed.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        [AttachedProperty(typeof(bool))]
        public static readonly ActiveDotvvmProperty ShowErrorMessageTextProperty =
            DelegateActionProperty<bool>.Register<ValidationMessage>("ShowErrorMessageText", AddShowErrorMessageText);

        private static void AddShowErrorMessageText(IHtmlWriter writer, RenderContext context, bool value, DotvvmControl control)
        {
            if (value)
            {
                var bindingGroup = new KnockoutBindingGroup();
                bindingGroup.Add("showErrorMessageText", "true");
                writer.AddKnockoutDataBind("dotvvmValidationOptions", bindingGroup);
            }
        }

        /// <summary>
        /// Gets or sets a binding that points to the validated value.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        [AttachedProperty(typeof(object))]
        public static readonly ActiveDotvvmProperty ValidatedValueProperty =
            DelegateActionProperty<object>.Register<ValidationMessage>("ValidatedValue", AddValidatedValue);

        private static void AddValidatedValue(IHtmlWriter writer, RenderContext context, object value, DotvvmControl control)
        {
            writer.AddKnockoutDataBind("dotvvmValidation", (DotvvmBindableControl)control, ValidatedValueProperty);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationMessage"/> class.
        /// </summary>
        public ValidationMessage()
        {
            TagName = "span";
            SetValue(HideWhenValidProperty, true);
        }

    }
}
