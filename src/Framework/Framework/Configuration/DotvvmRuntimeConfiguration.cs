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

        /// <summary> Whether DotVVM resources requests must have the correct version hash to be served, or any hash will be accepted by the server and possibly an unexpected resource version will be returned. </summary>
        /// <remarks>
        /// Enabling this option may improve security, as users should only be able to download scripts, which are used on pages which they can access.
        /// Unauthorized attacker will therefore only have access to the resources used on the login page.
        /// However, if application is updated while a user is loading a page, it may fail to load resources which have changed in the meantime.
        /// </remarks>
        [JsonPropertyName("requireResourceVersionHash")]
        public DotvvmGlobal3StateFeatureFlag RequireResourceVersionHash { get; } = new("RequireResourceVersionHash") { Enabled = null };

        /// <summary> Allows loading of *.map files from the same directory as the resource itself (for FileResourceLocation, or EmbeddedResourceLocation with DebugFilePath set). By default, this is enabled in debug mode. </summary>
        [JsonPropertyName("allowResourceMapFiles")]
        public DotvvmGlobal3StateFeatureFlag AllowResourceMapFiles { get; } = new("AllowResourceMapFiles") { Enabled = null };

        /// <summary> Whether DotVVM should add version hash to the resource URL and use immutable caching. Since v5.0, this is enabled by default in all configurations, the v4 behavior can be reverted by setting <c>AllowResourceVersionHash.Enable = null</c>. </summary>
        [JsonPropertyName("allowResourceVersionHash")]
        public DotvvmGlobal3StateFeatureFlag AllowResourceVersionHash { get; } = new("AllowResourceVersionHash") { Enabled = null };

        /// <summary>
        /// When enabled, the DotVVM runtime only automatically load assemblies listed in <see cref="DotvvmMarkupConfiguration.Assemblies"/>. This may prevent failures during startup and reduce startup time.
        /// See <see href="https://www.dotvvm.com/docs/4.0/pages/concepts/configuration/explicit-assembly-loading"> documentation page </see> for more information
        /// </summary>
        [JsonPropertyName("explicitAssemblyLoading")]
        public DotvvmGlobalFeatureFlag ExplicitAssemblyLoading { get; } = new DotvvmGlobalFeatureFlag("ExplicitAssemblyLoading");

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
            ExplicitAssemblyLoading.Freeze();
        }
    }
}
