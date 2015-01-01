using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Reference to a CSS file.
    /// </summary>
    public class StyleResource : RwResource
    {
        /// <summary>
        /// Gets or sets the URI of the CSS file.
        /// </summary>
        public string Uri { get; set; }


        public StyleResource(string uri, IEnumerable<string> dependencies)
        {
            this.Uri = uri;
            this.Dependencies = dependencies;
        }

        public StyleResource(string uri, params string[] dependencies) : this(uri, dependencies as IEnumerable<string>)
        {
        }

        /// <summary>
        /// Renders the reference to the page.
        /// </summary>
        public override void Render(IHtmlWriter writer)
        {
            writer.AddAttribute("href", Uri);
            writer.AddAttribute("rel", "stylesheet");
            writer.AddAttribute("type", "text/css");
            writer.RenderSelfClosingTag("link");
        }
    }
}
