using System.ComponentModel;
using Newtonsoft.Json;

namespace DotVVM.CommandLine.Core.Metadata
{
    public class ApiClientFetchOptions
    {

        [DefaultValue("same-origin")]
        [JsonProperty("credentials")]
        public string Credentials { get; set; } = "same-origin";

    }
}
