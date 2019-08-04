using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmExperimentalFeaturesConfiguration
    {

        // Add a property od DotvvmExperimentalFeatureFlag for each experimental feature here
        [JsonProperty("lazyCsrfToken", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmExperimentalFeatureFlag LazyCsrfToken { get; private set; } = new DotvvmExperimentalFeatureFlag();

    }

}
