using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Displays all validation messages from the current Validation.Target.
    /// </summary>
    public class ValidationSummary : HtmlGenericControl
    {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationSummary"/> class.
        /// </summary>
        public ValidationSummary()
        {
            TagName = "ul";
        }


        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            var expression = KnockoutHelper.GetValidationTargetExpression(this, true);
            if (expression != null)
            {
                writer.AddKnockoutDataBind("foreach", expression + ".$validationErrors");
            }

            base.AddAttributesToRender(writer, context);
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            // render template
            writer.AddKnockoutDataBind("text", "errorMessage");
            writer.RenderBeginTag("li");
            writer.RenderEndTag();
        }
    }
}
