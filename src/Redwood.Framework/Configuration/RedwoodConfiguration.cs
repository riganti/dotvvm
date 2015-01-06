using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Redwood.Framework.Routing;
using Redwood.Framework.Parser;
using Redwood.Framework.ResourceManagement;

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

        /// <summary>
        /// Gets the configuration of resources.
        /// </summary>
        [JsonProperty("resources")]
        [JsonConverter(typeof(ResourceRepositoryJsonConverter))]
        public RedwoodResourceRepository Resources { get; private set; }

        /// <summary>
        /// Gets the security configuration.
        /// </summary>
        [JsonProperty("security")]
        public RedwoodSecurityConfiguration Security { get; private set; }

        /// <summary>
        /// Gets the runtime configuration.
        /// </summary>
        [JsonProperty("runtime")]
        public RedwoodRuntimeConfiguration Runtime { get; private set; }

        /// <summary>
        /// Gets or sets the default culture.
        /// </summary>
        [JsonProperty("defaultCulture")]
        public string DefaultCulture { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodConfiguration"/> class.
        /// </summary>
        internal RedwoodConfiguration()
        {
            DefaultCulture = Thread.CurrentThread.CurrentCulture.Name;
            Markup = new RedwoodMarkupConfiguration();
            RouteTable = new RedwoodRouteTable(this);
            Resources = new RedwoodResourceRepository();
            Security = new RedwoodSecurityConfiguration();
            Runtime = new RedwoodRuntimeConfiguration();
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

            configuration.Resources.Register(
                new ScriptResource()
                {
                    Name = Constants.JQueryResourceName,
                    Url = "/Scripts/jquery-2.1.1.min.js",
                    CdnUrl = "https://code.jquery.com/jquery-2.1.1.min.js",
                    GlobalObjectName = "$"
                });
            configuration.Resources.Register(
                new ScriptResource()
                {
                    Name = Constants.KnockoutJSResourceName,
                    Url = "/Scripts/knockout-3.2.0.js",
                    GlobalObjectName = "ko"
                });
            configuration.Resources.Register(
                new ScriptResource()
                {
                    Name = Constants.KnockoutMapperResourceName,
                    Url = "/Scripts/knockout.mapper.js",
                    GlobalObjectName = "ko.mapper",
                    Dependencies = new[] { Constants.KnockoutJSResourceName }
                });
            configuration.Resources.Register(
                new ScriptResource()
                {
                    Name = Constants.RedwoodResourceName,
                    Url = "/Scripts/Redwood.js",
                    GlobalObjectName = "redwood",
                    Dependencies = new[] { Constants.KnockoutJSResourceName, Constants.KnockoutMapperResourceName }
                });
            configuration.Resources.Register(
                new ScriptResource()
                {
                    Name = Constants.BootstrapResourceName,
                    Url = "/Scripts/bootstrap.min.js",
                    CdnUrl = "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/js/bootstrap.min.js",
                    GlobalObjectName = "typeof $().emulateTransitionEnd == 'function'",
                    Dependencies = new[] { Constants.BootstrapCssResourceName, Constants.JQueryResourceName }
                });
            configuration.Resources.Register(
                new StylesheetResource()
                {
                    Name = Constants.BootstrapCssResourceName,
                    Url = "/Content/bootstrap/bootstrap.min.css"
                });

            return configuration;
        }
    }
}
