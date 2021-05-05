using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DotvvmByteArrayConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                var list = new List<byte>();
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.Integer:
                            list.Add((byte)reader.Value);
                            break;
                        case JsonToken.EndArray:
                            return list.ToArray();
                        default:
                            throw new FormatException($"Unexpected token while reading byte array: {reader.TokenType}");
                    }
                }

                throw new FormatException($"Unexpected end of array!");
            }
            else
            {
                throw new FormatException($"Expected StartArray token, but instead got {reader.TokenType}!");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var array = (byte[])value;
            writer.WriteStartArray();
            foreach (var item in array)
                writer.WriteValue(item);
            writer.WriteEndArray();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]);
        }
    }
}
