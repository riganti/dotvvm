using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls;

namespace Redwood.Framework.ResourceManagement
{
    /// <summary>
    /// Reference to a CSS file.
    /// </summary>
    [ResourceConfigurationCollectionName("stylesheets")]
    public class StylesheetResource : ResourceBase
    {

        /// <summary>
        /// Renders the resource in the specified <see cref="IHtmlWriter" />.
        /// </summary>
        public override void Render(IHtmlWriter writer)
        {
            writer.AddAttribute("href", Url);
            writer.AddAttribute("rel", "stylesheet");
            writer.AddAttribute("type", "text/css");
            writer.RenderSelfClosingTag("link");
        }
    }
}
