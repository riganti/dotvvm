using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class ClientExtenderInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("parameter")]
        public object Parameter { get; set; }
    }
}