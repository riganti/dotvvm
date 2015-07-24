using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmRuntimeConfiguration
    {

        /// <summary>
        /// Gets filters that are applied for all requests.
        /// </summary>
        [JsonIgnore()]
        public List<ActionFilterAttribute> GlobalFilters { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRuntimeConfiguration"/> class.
        /// </summary>
        public DotvvmRuntimeConfiguration()
        {
            GlobalFilters = new List<ActionFilterAttribute>();
        }
    }
}