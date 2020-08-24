using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DotVVM.CommandLine
{
    public class ApiClientFetchOptions
    {
        [DefaultValue("same-origin")]
        [JsonPropertyName("credentials")]
        public string? Credentials { get; set; } = "same-origin";
    }
}
