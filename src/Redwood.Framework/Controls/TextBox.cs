using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Renders a HTML text input control.
    /// </summary>
    public class TextBox : HtmlGenericControl
    {

        /// <summary>
        /// Gets or sets the text in the control.
        /// </summary>
        public string Text
        {
            get { return Convert.ToString(GetValue(TextProperty)); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly RedwoodProperty TextProperty =
            RedwoodProperty.Register<string, TextBox>(t => t.Text, "");


        /// <summary>
        /// Initializes a new instance of the <see cref="TextBox"/> class.
        /// </summary>
        public TextBox() : base("input")
        {
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddKnockoutDataBind("value", this, TextProperty, () =>
            {
                writer.AddAttribute("value", "Text");
            });
            writer.AddAttribute("type", "text");

            base.AddAttributesToRender(writer, context);
        }

    }
}
