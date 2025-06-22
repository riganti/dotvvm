using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

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
            writer.RenderBeginTag("link");
            writer.WriteAttributeUnbuffered("rel"u8, "preload"u8);
            writer.WriteAttributeUnbuffered("href"u8, Location.GetUrl(context, resourceName));
            writer.WriteAttributeUnbuffered("as"u8, "script"u8);
            writer.RenderEndTag();
        }
        protected override void RenderFallbackLoadingScript(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName, IResourceLocation fallback, string javascriptCondition)
        {
            using var text = new Utf8StringWriter(bufferSize: 2048);
            using var hw = new HtmlWriter(text, context);
            RenderLink(fallback, hw, context, resourceName, false);
            var link = StringUtils.Utf8Decode(text.PendingBytes);

            if (link.Length > 0)
            {
                var script = KnockoutHelper.MakeStringLiteral(link);
                var code = GetLoadingScript(javascriptCondition, script);
                if (Defer)
                {
                    writer.AddAttribute("defer", null);
                }
                InlineScriptResource.RenderDataUriString(writer, code.ToUtf8Bytes());
            }
        }
    }
}
