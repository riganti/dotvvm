using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Reference to a CSS file.
    /// </summary>
    public class StylesheetResource : LinkResourceBase
    {
        public StylesheetResource(IResourceLocation location)
            : base(ResourceRenderPosition.Head, "text/css", location)
        { }
        /// <remarks>Location property is required!</remarks>
        public StylesheetResource()
         : base(ResourceRenderPosition.Head, "text/css")
        { }

        public override void RenderLink(IResourceLocation location, IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            AddSrcAndIntegrity(writer, context, location.GetUrl(context, resourceName), "href");
            writer.AddAttribute("rel", "stylesheet");
            writer.AddAttribute("type", MimeType);
            writer.RenderSelfClosingTag("link");
        }
    }
}
