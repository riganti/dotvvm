using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DotvvmObjectConverter : JsonConverter<object?>
    {
        public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else if (typeof(object) == value.GetType())
            {
                writer.WriteStartObject();
                writer.WriteEndObject();
            }
            else
            {
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }

        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return SystemTextJsonUtils.DeserializeObject(ref reader, options);
        }
    }
}
