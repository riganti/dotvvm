using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Reference to a CSS file.
    /// </summary>
    [ResourceConfigurationCollectionName("stylesheets")]
    public class StylesheetResource : ResourceBase
    {

        public override ResourceRenderPosition GetRenderPosition()
        {
            return ResourceRenderPosition.Head;
        }

        /// <summary>
        /// Renders the resource in the specified <see cref="IHtmlWriter" />.
        /// </summary>
        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            writer.AddAttribute("href", GetUrl());
            writer.AddAttribute("rel", "stylesheet");
            writer.AddAttribute("type", "text/css");
            writer.RenderSelfClosingTag("link");
        }
    }
}
