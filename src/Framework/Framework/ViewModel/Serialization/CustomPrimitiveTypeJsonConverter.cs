using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DotvvmCustomPrimitiveTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return ReflectionUtils.CustomPrimitiveTypes.TryGetValue(objectType, out var result) && result is { };
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType is JsonToken.String
                or JsonToken.Boolean
                or JsonToken.Integer
                or JsonToken.Float
                or JsonToken.Date)
            {
                var registration = ReflectionUtils.CustomPrimitiveTypes[objectType]!;
                var parseResult = registration.TryParseMethod(Convert.ToString(reader.Value, CultureInfo.InvariantCulture));
                if (!parseResult.Successful)
                {
                    throw new JsonSerializationException($"The value '{reader.Value}' cannot be deserialized as {objectType} because its TryParse method wasn't able to parse the value!");
                }
                return parseResult.Result;
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                throw new JsonSerializationException($"Token {reader.TokenType} cannot be deserialized as {objectType}! Primitive value in JSON was expected.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var registration = ReflectionUtils.CustomPrimitiveTypes[value.GetType()]!;
                writer.WriteValue(registration.ToStringMethod(value));
            }
        }


    }
}
