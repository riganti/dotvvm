using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Controls.Infrastructure
{
    /// <summary>
    /// Renders the script elements and the serialized viewmodel. This control must be on every page just before the end of body element.
    /// </summary>
    public class BodyResourceLinks : DotvvmControl
    {

        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // render resource links
            var resources = context.ResourceManager.GetResourcesInOrder().Where(r => r.GetRenderPosition() == ResourceRenderPosition.Body);
            foreach (var resource in resources)
            {
                resource.Render(writer, context);
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
    window.dotvvm.init('root', '{Thread.CurrentThread.CurrentUICulture.Name}'); 
}});");
            writer.RenderEndTag();
        }
    }
}
