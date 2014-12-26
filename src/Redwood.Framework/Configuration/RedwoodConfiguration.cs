using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Redwood.Framework.Hosting;
using Redwood.Framework.Routing;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Configuration
{
    public class RedwoodConfiguration
    {
        public const string RedwoodControlTagPrefix = "rw";

        /// <summary>
        /// Gets or sets the application physical path.
        /// </summary>
        [JsonIgnore]
        public string ApplicationPhysicalPath { get; set; }

        /// <summary>
        /// Gets the settings of the markup.
        /// </summary>
        [JsonProperty("markup")]
        public RedwoodMarkupConfiguration Markup { get; private set; }

        /// <summary>
        /// Gets the route table.
        /// </summary>
        [JsonIgnore()]
        public RedwoodRouteTable RouteTable { get; private set; }

        public Controls.RwResourceRepository ResourceRepo { get; set; }
        /// <summary>
        /// Gets the security.
        /// </summary>
        public RedwoodSecurityConfiguration Security { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodConfiguration"/> class.
        /// </summary>
        internal RedwoodConfiguration()
        {
            Markup = new RedwoodMarkupConfiguration();
            RouteTable = new RedwoodRouteTable(this);
            ResourceRepo = new RwResourceRepository();
            Security = new RedwoodSecurityConfiguration();
        }

        /// <summary>
        /// Creates the default configuration.
        /// </summary>
        public static RedwoodConfiguration CreateDefault()
        {
            var configuration = new RedwoodConfiguration();
            configuration.Markup.Controls.AddRange(new[]
            {
                new RedwoodControlConfiguration() { TagPrefix = "rw", Namespace = "Redwood.Framework.Controls", Assembly = "Redwood.Framework" },
                new RedwoodControlConfiguration() { TagPrefix = "bootstrap", Namespace = "Redwood.Framework.Controls.Bootstrap", Assembly = "Redwood.Framework" },
            });
            return configuration;
        }
    }
}
