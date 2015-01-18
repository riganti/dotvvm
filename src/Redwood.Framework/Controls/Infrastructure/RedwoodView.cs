using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Runtime;
using Redwood.Framework.Parser;

namespace Redwood.Framework.Controls.Infrastructure
{
    /// <summary>
    /// Represents a top-level control in the control tree.
    /// </summary>
    public class RedwoodView : RedwoodBindableControl
    {

        /// <summary>
        /// Gets or sets the collection of directives.
        /// </summary>
        public Dictionary<string, string> Directives { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodView"/> class.
        /// </summary>
        public RedwoodView()
        {
            Directives = new Dictionary<string, string>();

            ResourceDependencies.Add(Constants.RedwoodResourceName);
            ResourceDependencies.Add(Constants.RedwoodValidationResourceName);
        }

        /// <summary>
        /// Renders the control begin tag.
        /// </summary>
        protected override void RenderBeginTag(IHtmlWriter writer, RenderContext context)
        {
            if (Directives.ContainsKey(Constants.DoctypeDirectiveName))
            {
                writer.WriteUnencodedText(string.Format("<!DOCTYPE {0}>\r\n", Directives[Constants.DoctypeDirectiveName]));
            }

            base.RenderBeginTag(writer, context);
        }

    }
}
