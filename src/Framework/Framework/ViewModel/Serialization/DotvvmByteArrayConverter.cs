using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DotvvmByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                var list = new List<byte>();
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.Number:
                            list.Add((byte)reader.GetUInt16());
                            break;
                        case JsonTokenType.EndArray:
                            return list.ToArray();
                        default:
                            throw new JsonException($"Unexpected token while reading byte array: {reader.TokenType}");
                    }
                }

                throw new JsonException($"Unexpected end of array!");
            }
            else
            {
                throw new JsonException($"Expected StartArray token, but instead got {reader.TokenType}!");
            }
        }

        public override void Write(Utf8JsonWriter writer, byte[] array, JsonSerializerOptions options)
        {
            if (array is null)
            {
                writer.WriteNullValue();
                return;
            }
            writer.WriteStartArray();
            foreach (var item in array)
                writer.WriteNumberValue(item);
            writer.WriteEndArray();
        }
    }
}
