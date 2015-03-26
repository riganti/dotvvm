using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Displays all validation messages from the current Validation.Target.
    /// </summary>
    public class ValidationSummary : HtmlGenericControl
    {
        public bool LocalOnly { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationSummary"/> class.
        /// </summary>
        public ValidationSummary()
        {
            TagName = "ul";
            LocalOnly = false;
        }


        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            var expression = KnockoutHelper.GetValidationTargetExpression(this);
            if (expression != null)
            {
                writer.AddKnockoutDataBind("foreach", LocalOnly? expression + ".$validationErrors" : "$root.$allValidationErrors");
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
