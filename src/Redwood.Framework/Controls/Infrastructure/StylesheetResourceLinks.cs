using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Configuration;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls.Infrastructure
{
    /// <summary>
    /// Renders the stylesheet links. This control must be on every page just before the end of head element.
    /// </summary>
    public class StylesheetResourceLinks : RedwoodControl
    {

        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            // render resource links
            var resources = context.ResourceManager.GetResourcesInCorrectOrder().Where(IsStylesheetResource);
            foreach (var resource in resources)
            {
                resource.Render(writer);
            }
        }

        private bool IsStylesheetResource(ResourceBase resource)
        {
            return resource is StylesheetResource;
        }
    }
}