using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Runtime.Tracing;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmRuntimeConfiguration
    {
        /// <summary>
        /// Gets filters that are applied for all requests.
        /// </summary>
        [JsonIgnore()]
        public IList<IActionFilter> GlobalFilters => _globalFilters;
        private IList<IActionFilter> _globalFilters;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRuntimeConfiguration"/> class.
        /// </summary>
        public DotvvmRuntimeConfiguration()
        {
            _globalFilters = new FreezableList<IActionFilter>();
        }

        private bool isFrozen = false;

        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error(nameof(DotvvmRuntimeConfiguration));
        }
        public void Freeze()
        {
            this.isFrozen = true;
            FreezableList.Freeze(ref _globalFilters);
        }
    }
}
