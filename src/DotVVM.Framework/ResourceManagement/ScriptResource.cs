using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Reference to a javascript file.
    /// </summary>
    [ResourceConfigurationCollectionName("scripts")]    
    public class ScriptResource : LinkResourceBase
    {
        private const string CdnFallbackScript = "if (typeof {0} === 'undefined') {{ document.write(\"<script src='{1}' type='text/javascript'><\\/script>\"); }}";

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
			AddAsyncAttribute(writer);
			AddDeferAttribute(writer);
            writer.RenderBeginTag("script");
            writer.RenderEndTag();
        }
    }
}
