using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a text into the page.
    /// </summary>
    [ControlMarkupOptions(DefaultContentProperty = nameof(Html))]
    public class HtmlLiteral : ConfigurableHtmlControl
    {

        /// <summary>
        /// Gets or sets the HTML that will be rendered in the control.
        /// </summary>
        public string Html
        {
            get { return (string)GetValue(HtmlProperty)!; }
            set { SetValue(HtmlProperty, value ?? throw new ArgumentNullException(nameof(value))); }
        }
        public static readonly DotvvmProperty HtmlProperty =
            DotvvmProperty.Register<string, HtmlLiteral>(t => t.Html, "");

        public HtmlLiteral() : base("div")
        {
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            TagName = WrapperTagName;

            base.AddAttributesToRender(writer, context);

            if (!RenderWrapperTag && !RenderOnServer && HasValueBinding(HtmlProperty))
            {
                throw new DotvvmControlException(this, "The HtmlLiteral control doesn't support client-side rendering without wrapper tag. Enable server rendering or the wrapper tag.");
            }
            
            if (RenderWrapperTag && HasValueBinding(HtmlProperty))
            {
                writer.AddKnockoutDataBind("html", this, HtmlProperty);
            }
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderWrapperTag)
            {
                base.RenderBeginTag(writer, context);
            }
            else if (!RenderOnServer && HasValueBinding(HtmlProperty))
            {
                writer.WriteKnockoutDataBindComment("html", this, HtmlProperty);
            }
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderOnServer || !HasValueBinding(HtmlProperty))
            {
                writer.WriteUnencodedText(Html);       
            }
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderWrapperTag)
            {
                base.RenderEndTag(writer, context);
            }
            else if (!RenderOnServer && HasValueBinding(HtmlProperty))
            {
                writer.WriteKnockoutDataBindEndComment();
            }
        }
    }
}
