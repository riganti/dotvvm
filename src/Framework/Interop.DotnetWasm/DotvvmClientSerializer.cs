using System.Text.Json;

namespace DotVVM.Framework.Interop.DotnetWasm;

public class DotvvmClientSerializer
{
    public string Serialize(object value)
    {
        return JsonSerializer.Serialize(value);
    }

    public object? Deserialize(Type type, string json)
    {
        return JsonSerializer.Deserialize(json, type);
    }
}
