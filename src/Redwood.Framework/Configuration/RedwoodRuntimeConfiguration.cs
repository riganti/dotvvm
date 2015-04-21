using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Redwood.Framework.Runtime.Filters;

namespace Redwood.Framework.Configuration
{
    public class RedwoodRuntimeConfiguration
    {

        /// <summary>
        /// Gets filters that are applied for all requests.
        /// </summary>
        [JsonIgnore()]
        public List<ActionFilterAttribute> GlobalFilters { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodRuntimeConfiguration"/> class.
        /// </summary>
        public RedwoodRuntimeConfiguration()
        {
            GlobalFilters = new List<ActionFilterAttribute>();
        }
    }
}