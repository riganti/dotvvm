using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the hyperlink which behaves like a button.
    /// </summary>
    public class LinkButton : ButtonBase
    {
        public LinkButton() : base("a")
        {
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            writer.AddAttribute("href", "#");

            var clickBinding = GetCommandBinding(ClickProperty);
            if (clickBinding != null)
            {
                writer.AddAttribute("onclick", KnockoutHelper.GenerateClientPostBackScript(clickBinding, context, this));
            }

            writer.AddKnockoutDataBind("text", this, TextProperty, () => { });
            
            base.AddAttributesToRender(writer, context);
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            var textBinding = GetBinding(TextProperty);
            if (textBinding == null && !string.IsNullOrEmpty(Text))
            {
                // render static value of the text property
                writer.WriteText(Text);
            }
            else
            {
                // render control contents
                RenderChildren(writer, context);
            }
        }
    }
}
