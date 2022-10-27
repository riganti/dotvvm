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
        /// Gets types that are treated as primitive by DotVVM components and serialization.
        /// </summary>
        [JsonIgnore()]
        public IList<CustomPrimitiveTypeRegistration> CustomPrimitiveTypes => _customPrimitiveTypes;
        private IList<CustomPrimitiveTypeRegistration> _customPrimitiveTypes;


        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRuntimeConfiguration"/> class.
        /// </summary>
        public DotvvmRuntimeConfiguration()
        {
            _globalFilters = new FreezableList<IActionFilter>();
            _customPrimitiveTypes = new FreezableList<CustomPrimitiveTypeRegistration>();
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
            FreezableList.Freeze(ref _customPrimitiveTypes);
        }
    }
}
