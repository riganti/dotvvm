using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// This converter serializes Dictionary&lt;&gt; as List&lt;KeyValuePair&lt;,&gt;&gt; in order to make dictionaries work with knockout. 
    /// </summary>
    public class DotvvmDictionaryConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.Implements(typeof(IReadOnlyDictionary<,>));
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert.Implements(typeof(IReadOnlyDictionary<,>), out var dictionaryType))
            {
                return (JsonConverter?)Activator.CreateInstance(typeof(Converter<,,>).MakeGenericType(dictionaryType.GetGenericArguments().Append(typeToConvert).ToArray()));
            }
            return null;
        }

        class Converter<K, V, TDictionary> : JsonConverter<TDictionary>
            where TDictionary : IReadOnlyDictionary<K, V>
            where K: notnull
        {
            public override void Write(Utf8JsonWriter json, TDictionary value, JsonSerializerOptions options)
            {
                json.WriteStartArray();
                foreach (var item in value)
                {
                    json.WriteStartObject();
                    json.WritePropertyName("Key"u8);
                    JsonSerializer.Serialize(json, item.Key, options);
                    json.WritePropertyName("Value"u8);
                    JsonSerializer.Serialize(json, item.Value, options);
                    json.WriteEndObject();
                }
                json.WriteEndArray();
            }
            public override TDictionary? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                    throw new JsonException($"Expected StartArray, but got {reader.TokenType}.");
                reader.Read();
                var dict = new Dictionary<K, V>();
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    if (reader.TokenType != JsonTokenType.StartObject)
                        throw new JsonException($"Expected StartObject, but got {reader.TokenType}.");
                    reader.Read();
                    (K key, V value) item = default;
                    while (reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName)
                            throw new JsonException($"Expected PropertyName, but got {reader.TokenType}.");
                        
                        if (reader.ValueTextEquals("Key"u8))
                        {
                            reader.Read();
                            item.key = SystemTextJsonUtils.Deserialize<K>(ref reader, options)!;
                            reader.Read();
                        }
                        else if (reader.ValueTextEquals("Value"u8))
                        {
                            reader.Read();
                            item.value = SystemTextJsonUtils.Deserialize<V>(ref reader, options)!;
                            reader.Read();
                        }
                        else
                        {
                            reader.Read();
                            reader.Skip();
                            reader.Read();
                        }
                    }
                    dict.Add(item.key!, item.value);
                    reader.Read();
                }
                reader.Read();

                if (dict is TDictionary result)
                    return result;
                if (ImmutableDictionary<K, V>.Empty is TDictionary)
                    return (TDictionary)(object)dict.ToImmutableDictionary();
                throw new NotSupportedException($"Cannot create instance of {typeToConvert}.");
            }
        }

    }

}
