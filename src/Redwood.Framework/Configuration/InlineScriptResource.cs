using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Configuration
{
    /// <summary>
    /// Piece of javascript code that is used in the page.
    /// </summary>
    public class InlineScriptResource : ResourceBase
    {
        
        /// <summary>
        /// Gets or sets the javascript code that will be embedded in the page.
        /// </summary>
        public string Code { get; set; }


        /// <summary>
        /// Renders the resource in the specified <see cref="IHtmlWriter" />.
        /// </summary>
        public override void Render(IHtmlWriter writer)
        {
            writer.AddAttribute("type", "text/javascript");
            writer.RenderBeginTag("script");
            writer.WriteUnencodedText(Code);
            writer.RenderEndTag();
        }
    }
}