using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the resource links with RenderPosition = Head. This control must be on every page, usually just before the end of head element.
    /// </summary>
    public class HeadResourceLinks : DotvvmControl
    {
        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // render resource links
            var resourceManager = context.ResourceManager;
            if (resourceManager.HeadRendered) return;
            resourceManager.HeadRendered = true; // set the flag before the resources are rendered, so they can't add more resources to the list during the render
            foreach (var resource in resourceManager.GetNamedResourcesInOrder())
            {
                if (resource.Resource.RenderPosition == ResourceRenderPosition.Head)
                    resource.RenderResourceCached(writer, context);
            }
        }
    }
}