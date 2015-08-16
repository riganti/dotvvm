using System;
using System.Linq;
using System.Net;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using Newtonsoft.Json;
using DotVVM.Framework.Parser;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a text into the page.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class Literal : DotvvmBindableControl
    {

        /// <summary>
        /// Gets or sets a whether the control content should be HTML encoded.
        /// </summary>
        public bool HtmlEncode
        {
            get { return (bool)GetValue(HtmlEncodeProperty); }
            set { SetValue(HtmlEncodeProperty, value); }
        }
        public static readonly DotvvmProperty HtmlEncodeProperty =
            DotvvmProperty.Register<bool, Literal>(t => t.HtmlEncode, true);


        /// <summary>
        /// Gets or sets the text displayed in the control.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<string, Literal>(t => t.Text, "");


        [MarkupOptions(AllowBinding = false)]
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }
        public static readonly DotvvmProperty FormatStringProperty =
            DotvvmProperty.Register<string, Literal>(c => c.FormatString);


        protected virtual bool AlwaysRenderSpan
        {
            get { return false; }
        }


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


        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            if (!string.IsNullOrEmpty(FormatString))
            {
                context.ResourceManager.AddCurrentCultureGlobalizationResource();
            }
            base.OnPreRender(context);
        }
         

        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        protected override void RenderControl(IHtmlWriter writer, RenderContext context)
        {
            if ((bool)GetValue(Internal.IsCommentProperty))
            {
                writer.WriteUnencodedText("<!--");
                writer.WriteUnencodedText(Text);
                writer.WriteUnencodedText("-->");
                return;
            }

            var textBinding = GetBinding(TextProperty);
            if (textBinding != null && !RenderOnServer && textBinding.Javascript != null)
            {
                var expression = textBinding.Javascript;
                if (!string.IsNullOrEmpty(FormatString))
                {
                    expression = "dotvvm.formatString(" + JsonConvert.SerializeObject(FormatString) + ", " + expression + ")";
                }

                writer.AddKnockoutDataBind(HtmlEncode ? "text" : "html", expression);
                AddAttributesToRender(writer, context);
                writer.RenderBeginTag("span");
                writer.RenderEndTag();
            }
            else
            {
                if (AlwaysRenderSpan)
                {
                    AddAttributesToRender(writer, context);
                    writer.RenderBeginTag("span");
                }

                var textToDisplay = "";
                if (!string.IsNullOrEmpty(FormatString))
                {
                    textToDisplay = string.Format("{0:" + FormatString + "}", GetValue(TextProperty));
                }
                else
                {
                    textToDisplay = GetValue(TextProperty)?.ToString() ?? "";
                }

                if (HtmlEncode)
                {
                    writer.WriteText(textToDisplay);
                }
                else
                {
                    writer.WriteUnencodedText(textToDisplay);
                }

                if (AlwaysRenderSpan)
                {
                    writer.RenderEndTag();
                }
            }
        }


        /// <summary>
        /// Determines whether the control contains only white space.
        /// </summary>
        public bool HasWhiteSpaceContentOnly()
        {
            if ((bool)GetValue(Internal.IsCommentProperty)) return true;

            var unencodedValue = HtmlEncode ? Text : WebUtility.HtmlDecode(Text);
            return unencodedValue.All(char.IsWhiteSpace);
        }
    }
}