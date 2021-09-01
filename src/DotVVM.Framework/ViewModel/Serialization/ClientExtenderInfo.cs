using System;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class ClientExtenderInfo
    {
        public ClientExtenderInfo(string name, object? parameter)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameter = parameter;
        }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("parameter")]
        public object? Parameter { get; set; }
    }
}
