#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
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
        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if ((HasBinding(TextProperty) || !string.IsNullOrEmpty(Text)) && !HasOnlyWhiteSpaceContent())
            {
                throw new DotvvmControlException(this, "The <dot:LinkButton> control cannot have both inner content and the Text property set!");
            }

            writer.AddAttribute("href", "javascript:;");

			var textbinding = GetValueBinding(TextProperty);
			if (textbinding != null) writer.AddKnockoutDataBind("text", textbinding.GetKnockoutBindingExpression(this));
            
            base.AddAttributesToRender(writer, context);

            var clickBinding = GetCommandBinding(ClickProperty);
            if (clickBinding != null)
            {
                writer.AddAttribute("onclick", KnockoutHelper.GenerateClientPostBackScript(nameof(Click), clickBinding, this), true, ";");
            }
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (!HasValueBinding(TextProperty) || RenderOnServer)
            {
                if (!string.IsNullOrEmpty(Text))
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
}
