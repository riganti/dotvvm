#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A common base for button controls.
    /// </summary>
    public abstract class ButtonBase : HtmlGenericControl, IEventValidationHandler
    {

        /// <summary>
        /// Gets or sets the text on the button.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty)!; }
            set { SetValue(TextProperty, value ?? throw new ArgumentNullException(nameof(value))); }
        }
        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<string, ButtonBase>(t => t.Text, "");


        /// <summary>
        /// Gets or sets the command that will be triggered when the button is clicked.
        /// </summary>
        public Command? Click
        {
            get { return (Command?)GetValue(ClickProperty); }
            set { SetValue(ClickProperty, value); }
        }
        public static readonly DotvvmProperty ClickProperty =
            DotvvmProperty.Register<Command?, ButtonBase>(t => t.Click, null);


        /// <summary>
        /// Gets or sets a value indicating whether the button is enabled and can be clicked on.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty)!; }
            set {  SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty =
            DotvvmPropertyWithFallback.Register<bool, ButtonBase>(nameof(Enabled), FormControls.EnabledProperty);


        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonBase"/> class.
        /// </summary>
        public ButtonBase(string tagName) : base(tagName)
        {
        }


        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddKnockoutDataBind("dotvvm-enable", this, EnabledProperty, () =>
            {
                if (!Enabled)
                {
                    writer.AddAttribute("disabled", "disabled");
                }
            });

            base.AddAttributesToRender(writer, context);
        }

        /// <summary>
        /// Determines whether it is legal to invoke a command on the specified property.
        /// </summary>
        public bool ValidateCommand(DotvvmProperty targetProperty)
        {
            if (targetProperty == ClickProperty)
            {
                return Enabled && Visible;
            }
            return false;
        }
    }
}
