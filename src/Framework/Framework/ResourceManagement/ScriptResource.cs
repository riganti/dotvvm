using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Reference to a javascript file.
    /// </summary>
    public class ScriptResource : LinkResourceBase, IPreloadResource, IDeferrableResource
    {
        public bool Defer { get; }
        public ScriptResource(IResourceLocation location, bool defer = true)
            : base(defer ? ResourceRenderPosition.Anywhere : ResourceRenderPosition.Body, "text/javascript", location)
        {
            this.Defer = defer;
        }

        /// <summary>Location property is required!</summary>
        public ScriptResource()
            : this(location: null!) // hack: people assign the Location property late, but it should non-nullable...
        { }

        public override void RenderLink(IResourceLocation location, IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            RenderLink(location, writer, context, resourceName, Defer);
        }

        private void RenderLink(IResourceLocation location, IHtmlWriter writer, IDotvvmRequestContext context, string resourceName, bool defer)
        {
            AddSrcAndIntegrity(writer, context, location.GetUrl(context, resourceName), "src");
            if (MimeType != "text/javascript") // this is the default, no need to write it
                writer.AddAttribute("type", MimeType);
            if (defer)
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
        protected override void RenderFallbackLoadingScript(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName, IResourceLocation fallback, string javascriptCondition)
        {
            var text = new StringWriter();
            var hw = new HtmlWriter(text, context);
            RenderLink(fallback, hw, context, resourceName, false);
            var link = text.ToString();

            if (!string.IsNullOrEmpty(link))
            {
                var script = JsonConvert.ToString(link, '\'').Replace("<", "\\u003c");
                var code = GetLoadingScript(javascriptCondition, script);
                if (Defer)
                {
                    writer.AddAttribute("defer", null);
                }
                InlineScriptResource.RenderDataUriString(writer, code);
            }
        }
    }
}
