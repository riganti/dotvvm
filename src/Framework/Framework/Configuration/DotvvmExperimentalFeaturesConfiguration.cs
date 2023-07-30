using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmExperimentalFeaturesConfiguration
    {

        // Add a DotvvmExperimentalFeatureFlag property for each experimental feature here

        /// <summary>
        /// When enabled, the CSRF token is not generated in each document, but lazy loaded when the first postback is performed. This may help with caching DotVVM pages.
        /// See <see href="https://www.dotvvm.com/blog/63/DotVVM-2-4-0-preview01-with-support-for-NET-Core-3-0"> DotVVM 2.4 release blog post </see> for more information
        /// </summary>
        [JsonProperty("lazyCsrfToken")]
        public DotvvmFeatureFlag LazyCsrfToken { get; private set; } = new DotvvmFeatureFlag("LazyCsrfToken");


        /// <summary>
        /// When enabled, each view model is stored server-side and client thus only need to send the differences. This reduces amount of data client have to upload at the expense of server memory.
        /// The view models are stored using the <see cref="DotVVM.Framework.ViewModel.Serialization.IViewModelServerStore" /> service.
        /// See <see href="https://www.dotvvm.com/docs/latest/pages/concepts/viewmodels/server-side-viewmodel-cache"> documentation page </see> for more information.
        /// </summary>
        [JsonProperty("serverSideViewModelCache")]
        public DotvvmFeatureFlag ServerSideViewModelCache { get; private set; } = new DotvvmFeatureFlag("ServerSideViewModelCache");

        /// <summary>
        /// When enabled, the DotVVM runtime only automatically load assemblies listed in <see cref="DotvvmMarkupConfiguration.Assemblies"/>. This may prevent failures during startup and reduce startup time.
        /// See <see href="https://www.dotvvm.com/docs/4.0/pages/concepts/configuration/explicit-assembly-loading"> documentation page </see> for more information
        /// </summary>
        [JsonProperty("explicitAssemblyLoading")]
        public DotvvmGlobalFeatureFlag ExplicitAssemblyLoading { get; private set; } = new DotvvmGlobalFeatureFlag("ExplicitAssemblyLoading");


        /// <summary>
        /// When enabled, knockout subscriptions are evaluated asynchronously. This may significantly improve client-side performance, but some component might not be compatible with the setting.
        /// <see href="https://knockoutjs.com/documentation/deferred-updates.html" />
        /// </summary>
        [JsonProperty("knockoutDeferUpdates")]
        public DotvvmFeatureFlag KnockoutDeferUpdates { get; private set; } = new DotvvmFeatureFlag("KnockoutDeferUpdates");

        [JsonProperty("useDotvvmSerializationForStaticCommandArguments")]
        public DotvvmGlobalFeatureFlag UseDotvvmSerializationForStaticCommandArguments { get; private set; } = new DotvvmGlobalFeatureFlag("UseDotvvmSerializationForStaticCommandArguments");

        public void Freeze()
        {
            LazyCsrfToken.Freeze();
            ServerSideViewModelCache.Freeze();
            ExplicitAssemblyLoading.Freeze();
            UseDotvvmSerializationForStaticCommandArguments.Freeze();
        }
    }

}
