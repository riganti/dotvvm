using System;
using System.Linq;
using System.Net;
using Redwood.Framework.Binding;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;
using Newtonsoft.Json;
using Redwood.Framework.Parser;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Renders a text into the page.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
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


        [MarkupOptions(AllowBinding = false)]
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }
        public static readonly RedwoodProperty FormatStringProperty =
            RedwoodProperty.Register<string, Literal>(c => c.FormatString);




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
        protected override void RenderControl(IHtmlWriter writer, RenderContext context)
        {
            var textBinding = GetBinding(TextProperty);
            if (textBinding != null && !RenderOnServer)
            {
                var expression = textBinding.TranslateToClientScript(this, TextProperty);
                if (!string.IsNullOrEmpty(FormatString))
                {
                    expression = "redwood.formatString(" + JsonConvert.SerializeObject(FormatString) + ", " + expression + ")";
                    context.ResourceManager.AddCurrentCultureGlobalizationResource();
                }

                writer.AddKnockoutDataBind(HtmlEncode ? "text" : "html", expression);
                writer.RenderBeginTag("span");
                writer.RenderEndTag();
            }
            else
            {
                var textToDisplay = Text;
                if (!string.IsNullOrEmpty(FormatString))
                {
                    textToDisplay = string.Format("{0:" + FormatString + "}", textToDisplay);
                }

                if (HtmlEncode)
                {
                    writer.WriteText(textToDisplay);
                }
                else
                {
                    writer.WriteUnencodedText(textToDisplay);
                }
            }
        }


        /// <summary>
        /// Determines whether the control contains only white space.
        /// </summary>
        public bool HasWhiteSpaceContentOnly()
        {
            var unencodedValue = HtmlEncode ? Text : WebUtility.HtmlDecode(Text);
            return unencodedValue.All(char.IsWhiteSpace);
        }
    }
}