using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Configuration;
using Redwood.Framework.Runtime;
using Redwood.Framework.ResourceManagement;

namespace Redwood.Framework.Controls.Infrastructure
{
    /// <summary>
    /// Renders the stylesheet links. This control must be on every page just before the end of head element.
    /// </summary>
    public class HeadResourceLinks : RedwoodControl
    {

        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        protected override void RenderControl(IHtmlWriter writer, RenderContext context)
        {
            // render resource links
            var resources = context.ResourceManager.GetResourcesInCorrectOrder().Where(r => r.GetRenderPosition() == ResourceRenderPosition.Head);
            foreach (var resource in resources)
            {
                resource.Render(writer);
            }
        }
    }
}