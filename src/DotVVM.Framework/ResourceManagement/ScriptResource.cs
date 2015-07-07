using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Parser;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Reference to a javascript file.
    /// </summary>
    [ResourceConfigurationCollectionName("scripts")]    
    public class ScriptResource : ResourceBase
    {
        private const string CdnFallbackScript = "{0} || document.write(\"<script src='{1}' type='text/javascript'><\\/script>\")";
        

        /// <summary>
        /// Gets or sets the URL of the script in CDN.
        /// </summary>
        [JsonProperty("cdnUrl")]
        public string CdnUrl { get; set; }

        /// <summary>
        /// Gets or sets the javascript expression that check if script was loaded (typically name of the object created by library in the global scope)
        /// It is used to check whether script from CDN was loaded or whether to load it from local URL.
        /// </summary>
        [JsonProperty("globalObjectName")]
        public string GlobalObjectName { get; set; }



        public ResourceRenderPosition RenderPosition { get; set; }

        public override ResourceRenderPosition GetRenderPosition()
        {
            return RenderPosition;
        }

        public ScriptResource()
        {
            RenderPosition = ResourceRenderPosition.Body;
        }

        /// <summary>
        /// Renders the resource in the specified <see cref="IHtmlWriter" />.
        /// </summary>
        public override void Render(IHtmlWriter writer)
        {
            if (CdnUrl != null)
            {
                writer.AddAttribute("src", CdnUrl);
                writer.AddAttribute("type", "text/javascript");
                writer.RenderBeginTag("script");
                writer.RenderEndTag();

                if (Url != null && GlobalObjectName != null)
                {
                    writer.RenderBeginTag("script");
                    writer.WriteUnencodedText(string.Format(CdnFallbackScript, GlobalObjectName, GetUrl()));
                    writer.RenderEndTag();
                }
            }
            else if (Url != null)
            {
                writer.AddAttribute("src", GetUrl());
                writer.AddAttribute("type", "text/javascript");
                writer.RenderBeginTag("script");
                writer.RenderEndTag();
            }
        }
    }
}
