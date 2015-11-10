using System;
using System.Linq;
using System.Net;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using Newtonsoft.Json;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a text into the page.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class Literal : DotvvmControl
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
            DotvvmProperty.Register<object, Literal>(t => t.Text, "");


        /// <summary>
        /// Gets or sets the format string that will be applied to numeric or date-time values.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }
        public static readonly DotvvmProperty FormatStringProperty =
            DotvvmProperty.Register<string, Literal>(c => c.FormatString);

        [MarkupOptions(AllowBinding = false)]
        public bool RenderSpanElement
        {
            get { return (bool)GetValue(RenderSpanElementProperty); }
            set { SetValue(RenderSpanElementProperty, value); }
        }
        public static readonly DotvvmProperty RenderSpanElementProperty =
            DotvvmProperty.Register<bool, Literal>(t => t.RenderSpanElement, false);


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
            var textBinding = GetValueBinding(TextProperty);
            if (textBinding != null && !RenderOnServer)
            {
                var expression = textBinding.GetKnockoutBindingExpression();
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
                if (RenderSpanElement)
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

                if (RenderSpanElement)
                {
                    writer.RenderEndTag();
                }
            }
        }
    }
}