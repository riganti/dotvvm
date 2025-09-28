using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Reference to a javascript file.
    /// </summary>
    public class ScriptModuleResource : LinkResourceBase, IPreloadResource, IDeferrableResource
    {
        [Obsolete("We don't support IE anymore", error: true)]
        public IResourceLocation? NomoduleLocation { get; }


        [Obsolete("<script type='module'> is always deferred, the attribute does nothing")]
        public bool Defer { get; }
        bool IDeferrableResource.Defer => true;

        public ScriptModuleResource(IResourceLocation location)
            : base(ResourceRenderPosition.Anywhere, "text/javascript", location)
        {
        }

        [Obsolete("<script type='module'> is always deferred, the attribute does nothing")]
        public ScriptModuleResource(IResourceLocation location, bool defer)
            : base(defer ? ResourceRenderPosition.Anywhere : ResourceRenderPosition.Body, "text/javascript", location)
        {
            this.Defer = defer;
        }

        public override void RenderLink(IResourceLocation location, IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            AddSrcAndIntegrity(writer, context, location.GetUrl(context, resourceName), "src");
            AddFetchPriority(writer, location, FetchPriority);
            writer.AddAttribute("type", "module");
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
