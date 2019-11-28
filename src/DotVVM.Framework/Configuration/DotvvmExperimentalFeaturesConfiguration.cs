#nullable enable
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmExperimentalFeaturesConfiguration
    {

        // Add a DotvvmExperimentalFeatureFlag property for each experimental feature here
        [JsonProperty("lazyCsrfToken", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmExperimentalFeatureFlag LazyCsrfToken { get; private set; } = new DotvvmExperimentalFeatureFlag();

        [JsonProperty("serverSideViewModelCache", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmExperimentalFeatureFlag ServerSideViewModelCache { get; private set; } = new DotvvmExperimentalFeatureFlag();

        public void Freeze()
        {
            LazyCsrfToken.Freeze();
        }
    }

}
