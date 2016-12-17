using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls.Infrastructure
{
    /// <summary>
    /// Renders the stylesheet links. This control must be on every page just before the end of head element.
    /// </summary>
    public class HeadResourceLinks : DotvvmControl
    {

        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // render resource links
            var resources = context.ResourceManager.GetNamedResourcesInOrder().Where(r => r.Resource.RenderPosition == ResourceRenderPosition.Head);
            foreach (var resource in resources)
            {
                resource.RenderResourceCached(writer, context);
            }
        }
    }
}