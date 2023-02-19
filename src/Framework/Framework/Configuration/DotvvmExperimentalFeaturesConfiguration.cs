using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmExperimentalFeaturesConfiguration
    {

        // Add a DotvvmExperimentalFeatureFlag property for each experimental feature here
        [JsonProperty("lazyCsrfToken")]
        public DotvvmFeatureFlag LazyCsrfToken { get; private set; } = new DotvvmFeatureFlag();

        [JsonProperty("serverSideViewModelCache")]
        public DotvvmFeatureFlag ServerSideViewModelCache { get; private set; } = new DotvvmFeatureFlag();

        [JsonProperty("explicitAssemblyLoading")]
        public DotvvmGlobalFeatureFlag ExplicitAssemblyLoading { get; private set; } = new DotvvmGlobalFeatureFlag();

        [JsonProperty("useDotvvmSerializationForStaticCommandArguments")]
        public DotvvmGlobalFeatureFlag UseDotvvmSerializationForStaticCommandArguments { get; private set; } = new DotvvmGlobalFeatureFlag();

        public void Freeze()
        {
            LazyCsrfToken.Freeze();
            ServerSideViewModelCache.Freeze();
            ExplicitAssemblyLoading.Freeze();
            UseDotvvmSerializationForStaticCommandArguments.Freeze();
        }
    }

}
