using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;

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
                return (JsonConverter?)Activator.CreateInstance(typeof(Converter<,,>).MakeGenericType([ ..dictionaryType.GetGenericArguments(), typeToConvert ]));
            }
            return null;
        }

        class Converter<K, V, TDictionary> : JsonConverter<TDictionary>, IDotvvmJsonConverter<TDictionary>
            where TDictionary : IReadOnlyDictionary<K, V>
            where K: notnull
        {
            private JsonConverter? defaultValueConverter;
            private readonly bool polymorphic = typeof(V).IsAbstract || typeof(V) == typeof(object);

            public override TDictionary? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                ReadInternal(ref reader, typeToConvert, options, DotvvmSerializationState.Current!);

            public TDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, DotvvmSerializationState state) =>
                ReadInternal(ref reader, typeToConvert, options, state);

            public TDictionary Populate(ref Utf8JsonReader reader, Type typeToConvert, TDictionary value, JsonSerializerOptions options, DotvvmSerializationState state)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return default!;

                if (value == null)
                    return ReadInternal(ref reader, typeToConvert, options, state);

                var newDict = ReadDictionaryItems(ref reader, value, options, state);
                if (value is IDictionary<K, V> mutableDict && !mutableDict.IsReadOnly)
                {
                    mutableDict.Clear();
                    foreach (var kvp in newDict)
                        mutableDict.Add(kvp.Key, kvp.Value);

                    return value;
                }
                else
                {
                    // immutable dict
                    return CreateDictionary(newDict, typeToConvert);
                }
            }

            private Dictionary<K, V> ReadDictionaryItems(ref Utf8JsonReader reader, IReadOnlyDictionary<K, V>? existingDict, JsonSerializerOptions options, DotvvmSerializationState state)
            {
                reader.AssertRead(JsonTokenType.StartArray);
                var newDict = new Dictionary<K, V>();

                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    var kvp = ReadKeyValuePair(ref reader, existingDict, options, state);
                    newDict.Add(kvp.Key, kvp.Value);
                    reader.AssertRead(JsonTokenType.EndObject);
                }

                return newDict;
            }

            private TDictionary CreateDictionary(Dictionary<K, V> dict, Type typeToConvert)
            {
                // Create the appropriate dictionary type
                if (dict is TDictionary result)
                    return result;
                if (ImmutableDictionary<K, V>.Empty is TDictionary)
                    return (TDictionary)(object)dict.ToImmutableDictionary();
                throw new NotSupportedException($"Cannot create instance of {typeToConvert.ToCode()}.");
            }

            private TDictionary ReadInternal(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, DotvvmSerializationState state)
            {
                var dict = ReadDictionaryItems(ref reader, null, options, state);
                return CreateDictionary(dict, typeToConvert);
            }

            private JsonConverter GetDefaultValueConverter(JsonSerializerOptions options)
            {
                if (defaultValueConverter is null)
                {
                    var conv = options.GetConverter(typeof(V));
                    Interlocked.CompareExchange(ref defaultValueConverter, conv, null);
                }
                return defaultValueConverter;
            }

            private KeyValuePair<K, V> ReadKeyValuePair(ref Utf8JsonReader reader, IReadOnlyDictionary<K, V>? existingDict, JsonSerializerOptions options, DotvvmSerializationState state)
            {
                reader.AssertRead(JsonTokenType.StartObject);
                K key = default!;
                V value = default!;
                bool hasKey = false, hasValue = false;
                
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    reader.AssertToken(JsonTokenType.PropertyName);
                    
                    if (reader.ValueTextEquals("Key"u8))
                    {
                        reader.AssertRead();
                        key = SystemTextJsonUtils.Deserialize<K>(ref reader, options)!;
                        hasKey = true;
                    }
                    else if (reader.ValueTextEquals("Value"u8))
                    {
                        reader.AssertRead();

                        if (existingDict is {} && hasKey && existingDict.TryGetValue(key!, out var existingValue) && existingValue is {})
                        {
                            if (polymorphic && existingValue.GetType() != typeof(V))
                            {
                                var valueConverter = options.GetConverter(existingValue.GetType());
                                if (valueConverter is IDotvvmJsonConverter dotvvmConverter)
                                    value = (V)dotvvmConverter.PopulateUntyped(ref reader, existingValue.GetType(), existingValue, options, state)!;
                                else
                                    value = SystemTextJsonUtils.Deserialize<V>(ref reader, options)!;
                            }
                            else
                            {
                                var valueConverter = GetDefaultValueConverter(options);
                                if (valueConverter is IDotvvmJsonConverter dotvvmConverter)
                                    value = (V)dotvvmConverter.PopulateUntyped(ref reader, typeof(V), existingValue, options, state)!;
                                else
                                    value = SystemTextJsonUtils.Deserialize<V>(ref reader, options)!;
                            }
                        }
                        else
                        {
                            var valueConverter = GetDefaultValueConverter(options);
                            if (valueConverter is IDotvvmJsonConverter dotvvmValueConverter)
                                value = (V)dotvvmValueConverter.ReadUntyped(ref reader, typeof(V), options, state)!;
                            else
                                value = SystemTextJsonUtils.Deserialize<V>(ref reader, options)!;
                        }
                        hasValue = true;
                    }
                    else
                    {
                        reader.AssertRead();
                        reader.Skip();
                    }
                    reader.AssertRead();
                }

                if (!hasKey || !hasValue) throw new JsonException("Missing Key or Value property in dictionary item.");
                return new KeyValuePair<K, V>(key, value);
            }

            public override void Write(Utf8JsonWriter json, TDictionary value, JsonSerializerOptions options) =>
                this.Write(json, value, options, null);

            public void Write(Utf8JsonWriter json, TDictionary value, JsonSerializerOptions options, DotvvmSerializationState? state, bool requireTypeField = true, bool wrapObject = true)
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

            public object? ReadUntyped(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, DotvvmSerializationState state) =>
                Read(ref reader, typeToConvert, options, state);

            public object? PopulateUntyped(ref Utf8JsonReader reader, Type typeToConvert, object? value, JsonSerializerOptions options, DotvvmSerializationState state)
            {
                if (value == null)
                    return Read(ref reader, typeToConvert, options, state);
                return Populate(ref reader, typeToConvert, (TDictionary)value, options, state);
            }

            public void WriteUntyped(Utf8JsonWriter writer, object? value, JsonSerializerOptions options, DotvvmSerializationState state, bool requireTypeField = true, bool wrapObject = true) =>
                Write(writer, (TDictionary)value!, options, state, requireTypeField, wrapObject);
        }

    }

}
