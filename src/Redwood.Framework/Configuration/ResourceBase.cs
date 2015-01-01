using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Configuration
{
    public abstract class ResourceBase
    {

        /// <summary>
        /// Gets or sets the name of the resource.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

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
        /// Initializes a new instance of the <see cref="ResourceBase"/> class.
        /// </summary>
        public ResourceBase()
        {
            Dependencies = new string[] { };
        }


        /// <summary>
        /// Renders the resource in the specified <see cref="IHtmlWriter"/>.
        /// </summary>
        public abstract void Render(IHtmlWriter writer);
    }
}
