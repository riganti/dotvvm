using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime;
using Newtonsoft.Json;

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
        public static readonly DotvvmProperty HideWhenValidProperty =
            DotvvmProperty.Register<bool, ValidationMessage>("HideWhenValid", isValueInherited: true);

        /// <summary>
        /// Gets or sets the name of CSS class which is applied to the control when it is not valid.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        [AttachedProperty(typeof(string))]
        public static readonly DotvvmProperty InvalidCssClassProperty =
            DotvvmProperty.Register<string, ValidationMessage>("InvalidCssClass", isValueInherited: true);

        /// <summary>
        /// Gets or sets whether the title attribute should be set to the error message.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        [AttachedProperty(typeof(bool))]
        public static readonly DotvvmProperty SetToolTipTextProperty =
            DotvvmProperty.Register<bool, ValidationMessage>("SetToolTipText", isValueInherited: true);

        /// <summary>
        /// Gets or sets whether the error message text should be displayed.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        [AttachedProperty(typeof(bool))]
        public static readonly DotvvmProperty ShowErrorMessageTextProperty =
            DotvvmProperty.Register<bool, ValidationMessage>("ShowErrorMessageText", isValueInherited: true);



        /// <summary>
        /// Gets or sets a binding that points to the validated value.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        [AttachedProperty(typeof(object))]
        public static readonly ActiveDotvvmProperty ValidatedValueProperty =
            DelegateActionProperty<object>.Register<ValidationMessage>("ValidatedValue", AddValidatedValue);




        public static List<DotvvmProperty> ValidationOptionProperties { get; } = new List<DotvvmProperty>()
        {
            HideWhenValidProperty,
            InvalidCssClassProperty,
            SetToolTipTextProperty,
            ShowErrorMessageTextProperty
        };




        private static void AddValidatedValue(IHtmlWriter writer, RenderContext context, object value, DotvvmControl control)
        {
            writer.AddKnockoutDataBind("dotvvmValidation", control, ValidatedValueProperty);

            // render options
            var bindingGroup = new KnockoutBindingGroup();
            foreach (var property in ValidationOptionProperties)
            {
                var javascriptName = property.Name.Substring(0, 1).ToLower() + property.Name.Substring(1);
                var optionValue = control.GetValue(property);
                if (!object.Equals(optionValue, property.DefaultValue))
                {
                    bindingGroup.Add(javascriptName, JsonConvert.SerializeObject(optionValue));
                }
            }
            writer.AddKnockoutDataBind("dotvvmValidationOptions", bindingGroup);
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
