using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
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
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly RedwoodProperty TextProperty =
            RedwoodProperty.Register<string, ButtonBase>(t => t.Text, "");


        /// <summary>
        /// Gets or sets the command that will be triggered when the button is pressed.
        /// </summary>
        public Action Click
        {
            get { return (Action)GetValue(ClickProperty); }
            set { SetValue(ClickProperty, value); }
        }
        public static readonly RedwoodProperty ClickProperty =
            RedwoodProperty.Register<Action, ButtonBase>(t => t.Click, null);


        /// <summary>
        /// Gets or sets a value indicating whether the button is enabled and can be clicked on.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set {  SetValue(EnabledProperty, value); }
        }
        public static readonly RedwoodProperty EnabledProperty =
            RedwoodProperty.Register<bool, ButtonBase>(t => t.Enabled, true);



        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonBase"/> class.
        /// </summary>
        public ButtonBase(string tagName) : base(tagName)
        {
        }


        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("enable", this, EnabledProperty, () =>
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
        public bool ValidateCommand(RedwoodProperty targetProperty)
        {
            if (targetProperty == ClickProperty)
            {
                return Enabled && Visible;
            }
            return false;
        }
    }
}