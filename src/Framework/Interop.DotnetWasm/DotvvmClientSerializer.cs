using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.Interop.DotnetWasm;

public class DotvvmClientSerializer
{
    private readonly JsonSerializerOptions options;

    public DotvvmClientSerializer()
    {
        this.options = GetDefaultOptions();
    }

    private JsonSerializerOptions GetDefaultOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public string Serialize(object? value)
    {
        return JsonSerializer.Serialize(value, options);
    }

    public object? Deserialize(Type type, string json)
    {
        return JsonSerializer.Deserialize(json, type, options);
    }
}
