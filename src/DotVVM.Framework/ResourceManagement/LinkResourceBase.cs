using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Base for resources linked from a location. Automatically renders failover script and contains helper methods for rendering urls and integrity hashes.
    /// </summary>
    public abstract class LinkResourceBase : ResourceBase, ILinkResource
    {
        public IResourceLocation Location { get; set; }
        public ResourceLocationFallback LocationFallback { get; set; }
        public string MimeType { get; set; } = "text/plain";

        public LinkResourceBase(ResourceRenderPosition renderPosition,
            string mimeType,
            IResourceLocation location)
            :base(renderPosition)
        {
            this.Location = location;
            this.MimeType = mimeType;
        }

        public IEnumerable<IResourceLocation> GetLocations()
        {
            yield return Location;
            if (LocationFallback != null) foreach (var l in LocationFallback.AlternativeLocations) yield return l;
        }

        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            RenderLink(Location, writer, context, resourceName);
            if (LocationFallback != null)
            {
                foreach (var fallback in LocationFallback.AlternativeLocations)
                {
                    var link = RenderLinkToString(fallback, context, resourceName);
                    if (!string.IsNullOrEmpty(link))
                    {
                        writer.AddAttribute("type", "text/javascript");
                        writer.RenderBeginTag("script");
                        writer.WriteUnencodedText($"{LocationFallback.JavascriptCondition} || document.write({JsonConvert.ToString(link, '\'').Replace("<", "\\u003c")})");
                        writer.RenderEndTag();
                    }
                }
            }
        }

        private string RenderLinkToString(IResourceLocation location, IDotvvmRequestContext context, string resourceName)
        {
            var text = new StringWriter();
            var writer = new HtmlWriter(text, context);
            RenderLink(location, writer, context, resourceName);
            return text.ToString();
        }

        public abstract void RenderLink(IResourceLocation location, IHtmlWriter writer, IDotvvmRequestContext context, string resourceName);

        protected string ComputeIntegrityHash(IDotvvmRequestContext context)
        {
            var hasher = context.Configuration.ServiceLocator.GetService<IResourceHashService>();
            var localLocation = GetLocations().OfType<ILocalResourceLocation>().First();
            if (localLocation != null) return hasher.GetIntegrityHash(localLocation, context);
            else return null;
        }

        protected void AddIntegrityAttribute(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var hash = ComputeIntegrityHash(context);
            if (hash != null) writer.AddAttribute("integrity", hash);
        }

        protected void AddSrcAndIntegrity(IHtmlWriter writer, IDotvvmRequestContext context, string url, string srcAttributeName)
        {
            writer.AddAttribute(srcAttributeName, url);
            if (url.Contains("://"))
            {
                AddIntegrityAttribute(writer, context);
            }
        }
    }

    public class ResourceLocationFallback
    {
        /// <summary>
        /// Javascript expression which return true (truthy value) when the script IS NOT correctly loaded
        /// </summary>
        public string JavascriptCondition { get; }
        public List<IResourceLocation> AlternativeLocations { get; }

        public ResourceLocationFallback(string javascriptCondition, params IResourceLocation[] alternativeLocations)
        {
            if (javascriptCondition == null) throw new ArgumentNullException(nameof(javascriptCondition));
            if (alternativeLocations == null) throw new ArgumentNullException(nameof(alternativeLocations));
            this.JavascriptCondition = javascriptCondition;
            this.AlternativeLocations = alternativeLocations.ToList();
        }
    }
}
