using System;
using Newtonsoft.Json;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CustomPrimitiveTypes
{
    public class TypeIdJsonConverter<TId> : JsonConverter where TId : TypeId<TId>
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TId); 
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var idText = (string)reader.Value;
                var idValue = Guid.Parse(idText);
                return TypeId<TId>.CreateExisting(idValue);
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                throw new JsonSerializationException($"Token {reader.TokenType} cannot be deserialized as TypeId!");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((value as ITypeId)?.IdValue);
        }

    }

}

