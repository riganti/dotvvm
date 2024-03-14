using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using DotVVM.Framework.Runtime.Filters;

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
        /// When enabled, dothtml files are reloaded and recompiled after a change. Note that resources (CSS, JS) are not controlled by this option.
        /// By default, reloading is only enabled in debug mode.
        /// </summary>
        [JsonPropertyName("reloadMarkupFiles")]
        public DotvvmGlobal3StateFeatureFlag ReloadMarkupFiles { get; } = new("Dotvvm3StateFeatureFlag.ReloadMarkupFiles");

        /// <summary>
        /// When enabled, command and staticCommand requests are compressed client-side and DotVVM accepts the compressed requests.
        /// It is enabled by default in Production mode.
        /// See <see cref="MaxPostbackSizeBytes" /> to limit the impact of potential decompression bomb. Although compression may be enabled only for specific routes, DotVVM does not check authentication before decompressing the request.
        /// </summary>
        [JsonPropertyName("compressPostbacks")]
        public Dotvvm3StateFeatureFlag CompressPostbacks { get; } = new("DotvvmFeatureFlag.CompressPostbacks");
        
        /// <summary> Maximum size of command/staticCommand request body after decompression (does not affect file upload). Default = 128MB, lower limit is a basic protection against decompression bomb attack. Set to -1 to disable the limit. </summary>
        [JsonPropertyName("maxPostbackSizeBytes")]
        public long MaxPostbackSizeBytes { get; set; } = 1024 * 1024 * 128; // 128 MB

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
            ReloadMarkupFiles.Freeze();
        }
    }
}
