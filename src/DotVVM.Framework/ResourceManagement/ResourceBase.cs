using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser;
using Newtonsoft.Json;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    public abstract class ResourceBase
    {
        /// <summary>
        /// Gets or sets the collection of dependent resources.
        /// </summary>
        [JsonProperty("dependencies")]
        public string[] Dependencies { get; set; }

        /// <summary>
        /// Gets or sets the local URL of the resource.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the name of the assembly. If this is set, the value of the <see cref="P:Url"/> property is the name of the embedded resource in the specified assembly.
        /// </summary>
        [JsonProperty("embeddedResourceAssembly")]
        public string EmbeddedResourceAssembly { get; set; }

        /// <summary>
        /// Gets a value indicating whether the script is an embedded resource.
        /// </summary>
        public bool IsEmbeddedResource
        {
            get { return !string.IsNullOrEmpty(EmbeddedResourceAssembly); }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceBase"/> class.
        /// </summary>
        public ResourceBase()
        {
            Dependencies = new string[] { };
        }

        /// <summary>
        /// Get where the resource want to be rendered
        /// </summary>
        public abstract ResourceRenderPosition GetRenderPosition();

        /// <summary>
        /// Renders the resource in the specified <see cref="IHtmlWriter"/>.
        /// </summary>
        public abstract void Render(IHtmlWriter writer, IDotvvmRequestContext context);



        /// <summary>
        /// Gets the URL.
        /// </summary>
        protected string GetUrl(IDotvvmRequestContext context)
        {
            if (IsEmbeddedResource)
            {
                return ResourceUrlGenerator.GetEmbeddedResourceUrl(this.Url, this.EmbeddedResourceAssembly);
            }

            return ResourceUrlGenerator.GetResourceUrl(context, Url);
        }
    }
}
