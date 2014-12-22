using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Redwood.Framework.Configuration
{
    public class RedwoodMarkupConfiguration
    {

        /// <summary>
        /// Gets the registered control namespaces.
        /// </summary>
        [JsonProperty("controls")]
        public List<RedwoodControlConfiguration> Controls { get; private set; }

        /// <summary>
        /// Gets or sets the list of referenced assemblies.
        /// </summary>
        [JsonProperty("assemblies")]
        public List<string> Assemblies { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodMarkupConfiguration"/> class.
        /// </summary>
        public RedwoodMarkupConfiguration()
        {
            Controls = new List<RedwoodControlConfiguration>();
            Assemblies = new List<string>();
        }

    }
}