using System;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class ClientExtenderInfo
    {
        public ClientExtenderInfo(string name, object? parameter)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameter = parameter;
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("parameter")]
        public object? Parameter { get; set; }
    }
}
