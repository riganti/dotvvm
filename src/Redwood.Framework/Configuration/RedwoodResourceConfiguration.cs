using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Redwood.Framework.Configuration
{
    /// <summary>
    /// Holds a configuration of named resources.
    /// </summary>
    public class RedwoodResourceConfiguration
    {

        [JsonProperty("scripts")]
        public List<ScriptResource> Scripts { get; private set; }


        [JsonProperty("stylesheets")]
        public List<StylesheetResource> Stylesheets { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodResourceConfiguration"/> class.
        /// </summary>
        public RedwoodResourceConfiguration()
        {
            Scripts = new List<ScriptResource>();
            Stylesheets = new List<StylesheetResource>();
        }


        /// <summary>
        /// Finds the resource with the specified name.
        /// </summary>
        public ResourceBase FindResource(string name)
        {
            return Scripts.Cast<ResourceBase>().Concat(Stylesheets).LastOrDefault(b => b.Name == name);     // last is important here - we need redwood.json entries to override the default ones
        }
    }

}