using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using Newtonsoft.Json;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.Runtime;

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
            ResourcesRenderer.RenderResources(resourceManager, writer, context, ResourceRenderPosition.Body);

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
            writer.WriteUnencodedText(RenderWarnings(context));
            writer.RenderEndTag();
        }

        internal static string RenderWarnings(IDotvvmRequestContext context)
        {
            var result = "";
            // propagate warnings to JS console
            var collector = context.Services.GetService<RuntimeWarningCollector>();
            if (!collector.Enabled) return result;

            foreach (var w in collector.GetWarnings())
            {
                var msg = JsonConvert.ToString(w.ToString(), '"', StringEscapeHandling.EscapeHtml);
                result += $"console.warn({msg});\n";
            }
            return result;
        }
    }
}
