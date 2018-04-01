using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.ResourceManagement;
using System.Globalization;
using Newtonsoft.Json;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders the resource links with RenderPosition = Body and the serialized viewmodel. This control must be on every page, usually just before the end of body element.
    /// </summary>
    public class BodyResourceLinks : DotvvmControl
    {
        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // render resource links
            var resourceManager = context.ResourceManager;
            if (resourceManager.BodyRendered) return;
            resourceManager.BodyRendered = true;  // set the flag before the resources are rendered, so they can't add more resources to the list during the render
            foreach (var resource in resourceManager.GetNamedResourcesInOrder())
            {
                if (resource.Resource.RenderPosition == ResourceRenderPosition.Body)
                    resource.RenderResourceCached(writer, context);
            }

            // render the serialized viewmodel
            var serializedViewModel = ((DotvvmRequestContext) context).GetSerializedViewModel();
            writer.AddAttribute("type", "hidden");
            writer.AddAttribute("id", "__dot_viewmodel_root");
            writer.AddAttribute("value", serializedViewModel);
            writer.RenderSelfClosingTag("input");

            // init on load
            writer.RenderBeginTag("script");
            writer.WriteUnencodedText($@"
window.dotvvm.domUtils.onDocumentReady(function () {{
    window.dotvvm.init('root', {JsonConvert.ToString(CultureInfo.CurrentCulture.Name, '"', StringEscapeHandling.EscapeHtml)});
}});");
            writer.RenderEndTag();
        }
    }
}
