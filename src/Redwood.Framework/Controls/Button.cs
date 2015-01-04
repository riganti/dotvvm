using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Renders a HTML button.
    /// </summary>
    public class Button : ButtonBase
    {

        /// <summary>
        /// Gets or sets whether the button should render as input[type=submit] or input[type=button]. 
        /// The submit button has some special features, e.g. handles the Return key in HTML forms etc.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool IsSubmitButton
        {
            get { return (bool)GetValue(IsSubmitButtonProperty); }
            set { SetValue(IsSubmitButtonProperty, value); }
        }
        public static readonly RedwoodProperty IsSubmitButtonProperty
            = RedwoodProperty.Register<bool, Button>(c => c.IsSubmitButton, false);



        /// <summary>
        /// Initializes a new instance of the <see cref="Button"/> class.
        /// </summary>
        public Button() : base("input")
        {
        }


        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddAttribute("type", IsSubmitButton ? "submit" : "button");

            var clickBinding = GetCommandBinding(ClickProperty);
            if (clickBinding != null)
            {
                writer.AddAttribute("onclick", KnockoutHelper.GenerateClientPostBackScript(clickBinding, context, this));
            }

            writer.AddKnockoutDataBind("value", this, TextProperty, () => writer.AddAttribute("value", Text));

            base.AddAttributesToRender(writer, context);
        }

    }
}
