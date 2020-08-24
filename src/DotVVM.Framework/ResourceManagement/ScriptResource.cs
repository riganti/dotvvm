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
    public class ScriptResource : LinkResourceBase, IPreloadResource
    {
        public ScriptType ScriptType { get; set; } = ScriptType.Standard;

        public ScriptResource(IResourceLocation location)
            : base(ResourceRenderPosition.Body, "text/javascript", location)
        { }

        /// <summary>Location property is required!</summary>
        public ScriptResource()
            : base(ResourceRenderPosition.Body, "text/javascript")
        { }

        public override void RenderLink(IResourceLocation location, IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            AddSrcAndIntegrity(writer, context, location.GetUrl(context, resourceName), "src");
            writer.AddAttribute("type", MimeType);
            switch (ScriptType)
            {
                case ScriptType.Defer:
                    writer.AddAttribute("defer",null);
                    break;
                case ScriptType.Async:
                    writer.AddAttribute("async", null);
                    break;
                default:
                    break;
            }
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
