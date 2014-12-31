using System;
using System.Linq;
using System.Net;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// A simple control that renders a text in the web page.
    /// </summary>
    public class Literal : RedwoodBindableControl
    {

        /// <summary>
        /// Gets or sets a whether the control content should be HTML encoded.
        /// </summary>
        public bool HtmlEncode
        {
            get { return (bool)GetValue(HtmlEncodeProperty); }
            set { SetValue(HtmlEncodeProperty, value); }
        }
        public static readonly RedwoodProperty HtmlEncodeProperty =
            RedwoodProperty.Register<bool, Literal>(t => t.HtmlEncode, true);


        /// <summary>
        /// Gets or sets the text displayed in the control.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly RedwoodProperty TextProperty =
            RedwoodProperty.Register<string, Literal>(t => t.Text, "");


        /// <summary>
        /// Initializes a new instance of the <see cref="Literal"/> class.
        /// </summary>
        public Literal()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Literal"/> class.
        /// </summary>
        public Literal(string text)
        {
            Text = text;
            HtmlEncode = false;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Literal"/> class.
        /// </summary>
        public Literal(string text, bool encode)
        {
            Text = text;
            HtmlEncode = encode;
        }


        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            var textBinding = GetBinding(TextProperty);
            if (textBinding != null)
            {
                writer.AddKnockoutDataBind(HtmlEncode ? "text" : "html", textBinding as ValueBindingExpression, this, TextProperty);
                writer.RenderBeginTag("span");
                writer.RenderEndTag();
            }
            else
            {
                if (HtmlEncode)
                {
                    writer.WriteText(Text);
                }
                else
                {
                    writer.WriteUnencodedText(Text);
                }
            }
        }


        /// <summary>
        /// Determines whether the literal contains only white space.
        /// </summary>
        public bool IsWhiteSpaceOnly()
        {
            var unencodedValue = HtmlEncode ? Text : WebUtility.HtmlDecode(Text);
            return unencodedValue.All(char.IsWhiteSpace);
        }
    }
}