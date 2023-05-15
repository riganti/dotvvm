using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmExperimentalFeaturesConfiguration
    {

        // Add a DotvvmExperimentalFeatureFlag property for each experimental feature here
        [JsonProperty("lazyCsrfToken")]
        public DotvvmFeatureFlag LazyCsrfToken { get; private set; } = new DotvvmFeatureFlag("LazyCsrfToken");

        [JsonProperty("serverSideViewModelCache")]
        public DotvvmFeatureFlag ServerSideViewModelCache { get; private set; } = new DotvvmFeatureFlag("ServerSideViewModelCache");

        [JsonProperty("explicitAssemblyLoading")]
        public DotvvmGlobalFeatureFlag ExplicitAssemblyLoading { get; private set; } = new DotvvmGlobalFeatureFlag("ExplicitAssemblyLoading");

        public void Freeze()
        {
            LazyCsrfToken.Freeze();
            ServerSideViewModelCache.Freeze();
            ExplicitAssemblyLoading.Freeze();
        }
    }

}
