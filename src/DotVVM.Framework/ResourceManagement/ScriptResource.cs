#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Reference to a javascript file.
    /// </summary>
    public class ScriptResource : LinkResourceBase, IPreloadResource, IDeferableResource
    {
        public bool Defer { get; }
        public ScriptResource(IResourceLocation location, bool defer = false)
            : base(defer ? ResourceRenderPosition.Anywhere : ResourceRenderPosition.Body, "text/javascript", location)
        {
            this.Defer = defer;
        }

        /// <summary>Location property is required!</summary>
        public ScriptResource()
            : base(ResourceRenderPosition.Body, "text/javascript")
        { }

        public override void RenderLink(IResourceLocation location, IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            AddSrcAndIntegrity(writer, context, location.GetUrl(context, resourceName), "src");
            if (MimeType != "text/javascript") // this is the default, no need to write it
                writer.AddAttribute("type", MimeType);
            if (Defer)
                writer.AddAttribute("defer", null);
            writer.RenderBeginTag("script");
            writer.RenderEndTag();
        }

        public void RenderPreloadLink(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            writer.AddAttribute("rel", "preload");
            writer.AddAttribute("href", Location.GetUrl(context, resourceName));
            writer.AddAttribute("as", "script");

            writer.RenderBeginTag("link");
            writer.RenderEndTag();
        }
    }
}
