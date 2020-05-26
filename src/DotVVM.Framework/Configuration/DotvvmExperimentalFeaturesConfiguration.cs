#nullable enable
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmExperimentalFeaturesConfiguration
    {

        // Add a DotvvmExperimentalFeatureFlag property for each experimental feature here
        [JsonProperty("lazyCsrfToken")]
        public DotvvmExperimentalFeatureFlag LazyCsrfToken { get; private set; } = new DotvvmExperimentalFeatureFlag();

        [JsonProperty("serverSideViewModelCache")]
        public DotvvmExperimentalFeatureFlag ServerSideViewModelCache { get; private set; } = new DotvvmExperimentalFeatureFlag();

        public void Freeze()
        {
            LazyCsrfToken.Freeze();
        }
    }

}
