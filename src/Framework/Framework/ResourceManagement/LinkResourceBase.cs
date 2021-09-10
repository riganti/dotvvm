﻿using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Base for resources linked from a location. Automatically renders failover script and contains helper methods for rendering urls and integrity hashes.
    /// </summary>
    public abstract class LinkResourceBase : ResourceBase, ILinkResource
    {
        /// <summary>Location property is required!</summary>
        public IResourceLocation Location { get; set; }
        public ResourceLocationFallback? LocationFallback { get; set; }
        public string MimeType { get; private set; }
        [DefaultValue(true)]
        public bool VerifyResourceIntegrity { get; set; } = true;
        public string? IntegrityHash { get; set; }

        public LinkResourceBase(ResourceRenderPosition renderPosition, string mimeType, IResourceLocation location) : base(renderPosition)
        {
            this.Location = location;
            this.MimeType = mimeType;
        }
        public LinkResourceBase(ResourceRenderPosition renderPosition, string mimeType) : base(renderPosition)
        {
            this.MimeType = mimeType;
            this.Location = null!; // TODO: deprecate this overload
        }

        public virtual IEnumerable<IResourceLocation> GetLocations(string? type = null)
        {
            if (type is object) yield break;

            yield return Location;
            if (LocationFallback != null)
            {
                foreach (var l in LocationFallback.AlternativeLocations)
                {
                    yield return l;
                }
            }
        }

        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        {
            RenderLink(Location, writer, context, resourceName);
            if (LocationFallback != null)
            {
                if (Location is ILocalResourceLocation
                    && LocationFallback.AlternativeLocations.Count > 0)
                {
                    throw new NotSupportedException("LocationFallback is not supported on " +
                        "resources with Location of type ILocalResourceLocation.");
                }

                foreach (var fallback in LocationFallback.AlternativeLocations)
                {
                    RenderFallbackLoadingScript(writer, context, resourceName, fallback, LocationFallback.JavascriptCondition);
                }
            }
        }

        protected virtual void RenderFallbackLoadingScript(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName, IResourceLocation fallback, string javascriptCondition)
        {
            var link = RenderLinkToString(fallback, context, resourceName);
            if (!string.IsNullOrEmpty(link))
            {
                writer.AddAttribute("type", "text/javascript");
                writer.RenderBeginTag("script");
                var script = JsonConvert.ToString(link, '\'').Replace("<", "\\u003c");
                writer.WriteUnencodedText(GetLoadingScript(javascriptCondition, script));
                writer.RenderEndTag();
            }
        }

        protected string GetLoadingScript(string javascriptCondition, string script) =>
        $@"if (!({javascriptCondition})) {{
    var wrapper = document.createElement('div');
    wrapper.innerHTML = {script};
    var originalScript = wrapper.children[0];
    var script = document.createElement('script');
    script.src = originalScript.src;
    script.type = originalScript.type;
    script.text = originalScript.text;
    script.id = originalScript.id;
    document.head.appendChild(script);
}}";

        protected string RenderLinkToString(IResourceLocation location, IDotvvmRequestContext context, string resourceName)
        {
            var text = new StringWriter();
            var writer = new HtmlWriter(text, context);
            RenderLink(location, writer, context, resourceName);
            return text.ToString();
        }

        public abstract void RenderLink(IResourceLocation location, IHtmlWriter writer, IDotvvmRequestContext context, string resourceName);

        protected string? ComputeIntegrityHash(IDotvvmRequestContext context)
        {
            var hasher = context.Services.GetRequiredService<IResourceHashService>();
            var localLocation = GetLocations().OfType<ILocalResourceLocation>().FirstOrDefault();
            if (localLocation != null) return hasher.GetIntegrityHash(localLocation, context);
            else return null;
        }

        protected void AddIntegrityAttribute(IHtmlWriter writer, IDotvvmRequestContext context, string url)
        {
            var hash = IntegrityHash ?? ComputeIntegrityHash(context);
            if (hash != null)
            {
                writer.AddAttribute("integrity", hash);
                writer.AddAttribute("crossorigin", "anonymous");
            }
            else
            {
                context.DebugWarning(
                    $"Resource seems to be pointing to a 3rd party URL '{url}', but it does not have IntegrityHash specified. Please specify this property or serve the resource from your server (using FileResourceLocation) to eliminate the potential security risk. See https://developer.mozilla.org/en-US/docs/Web/Security/Subresource_Integrity for more information."
                );
            }
        }

        protected void AddSrcAndIntegrity(IHtmlWriter writer, IDotvvmRequestContext context, string url, string srcAttributeName)
        {
            writer.AddAttribute(srcAttributeName, url);

            if (url.Contains("://") && VerifyResourceIntegrity)
            {
                AddIntegrityAttribute(writer, context, url);
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
