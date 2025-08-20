using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.Utils
{
    static class SystemTextJsonUtils
    {
        /// <summary> Returns the property path to an unfinished JSON value </summary>
        public static string[] GetFailurePath(ReadOnlySpan<byte> data)
        {
            // TODO: tests
            var reader = new Utf8JsonReader(data, false, default);
            var path = new Stack<(string? name, int index)>();
            var isArray = false;
            int arrayIndex = 0;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        if (isArray) {
                            isArray = false;
                            path.Push((null, arrayIndex));
                        }
                        break;
                    case JsonTokenType.Comment:
                        break;
                    case JsonTokenType.StartArray:
                        isArray = true;
                        arrayIndex = 0;
                        break;
                    case JsonTokenType.EndArray:
                        isArray = false;
                        break;
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Number:
                    case JsonTokenType.String:
                    case JsonTokenType.Null:
                    case JsonTokenType.EndObject:
                        if (!isArray) {
                            var old = path.Pop();
                            if (old.name is null) {
                                isArray = true;
                                arrayIndex = old.index + 1;
                            }
                        }
                        else {
                            arrayIndex++;
                        }
                        break;
                    case JsonTokenType.PropertyName:
                        path.Push((reader.GetString()!, -1));
                        break;
                    case JsonTokenType.None:
                        goto Done;
                }
            }
            Done:
            return path.Reverse().Select(n => n.name ?? n.index.ToString()).ToArray();
        }

        public static JsonElement? GetPropertyOrNull(this in JsonElement jsonObj, ReadOnlySpan<byte> name) =>
            jsonObj.TryGetProperty(name, out var prop) ? prop : null;
        public static JsonElement? GetPropertyOrNull(this in JsonElement jsonObj, string name) =>
            jsonObj.TryGetProperty(name, out var prop) ? prop : null;

        public static IEnumerable<string> EnumerateStringArray(this JsonElement json)
        {
            foreach (var item in json.EnumerateArray()) {
                yield return item.GetString()!;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssertToken(this in Utf8JsonReader reader, JsonTokenType type)
        {
            if (reader.TokenType != type)
                ThrowUnexpectedToken(in reader, type);
        }

        static void ThrowUnexpectedToken(in Utf8JsonReader reader, JsonTokenType expected)
        {
            var value = reader.TokenType is JsonTokenType.String or JsonTokenType.PropertyName or JsonTokenType.Number
                        ? $" (\"{StringUtils.Utf8Decode(reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan.ToArray())}\")"
                        : "";
            throw new JsonException($"Expected token of type {expected}, but got {reader.TokenType}{value} at position {reader.BytesConsumed}.");
        }

        public static void AssertRead(this ref Utf8JsonReader reader, JsonTokenType type)
        {
            AssertToken(in reader, type);
            AssertRead(ref reader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AssertRead(this ref Utf8JsonReader reader)
        {
            if (!reader.Read())
                ThrowUnexpectedEndOfStream(in reader);
        }

        public static void ThrowUnexpectedEndOfStream(in Utf8JsonReader reader)
        {
            throw new JsonException($"Unexpected end of stream at position {reader.BytesConsumed}.");
        }

        public static string? ReadString(this ref Utf8JsonReader reader)
        {
            if (reader.TokenType is JsonTokenType.Null)
            {
                return null!;
            }
            else if (reader.TokenType is JsonTokenType.String or JsonTokenType.PropertyName)
            {
                var value = reader.GetString()!;
                reader.AssertRead();
                return value;
            }
            else
            {
                throw new JsonException($"Expected string, but got {reader.TokenType}.");
            }
        }

        public static int GetValueLength(this in Utf8JsonReader reader)
        {
            return reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;
        }

        public static float GetFloat32Value(this in Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String)
                return (float)reader.GetStringFloatValue();
            return reader.GetSingle();
        }

        public static double GetFloat64Value(this in Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String)
                return reader.GetStringFloatValue();
            return reader.GetDouble();
        }

        public static double GetStringFloatValue(this in Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString()!;
                return stringValue switch {
                    "Infinity" => double.PositiveInfinity,
                    "-Infinity" => double.NegativeInfinity,
                    _ => double.Parse(stringValue)
                };
            }
            return reader.GetDouble();
        }

        public static void WriteFloatValue(Utf8JsonWriter writer, double number)
        {
#if DotNetCore
            if (double.IsFinite(number))
#else
            if (!double.IsInfinity(number) && !double.IsNaN(number))
#endif
                writer.WriteNumberValue(number);
            else
                WriteNonFiniteFloatValue(writer, (float)number);
        }
        public static void WriteFloatValue(Utf8JsonWriter writer, float number)
        {
#if DotNetCore
            if (float.IsFinite(number))
#else
            if (!float.IsInfinity(number) && !float.IsNaN(number))
#endif
                writer.WriteNumberValue(number);
            else
                WriteNonFiniteFloatValue(writer, number);
        }

        static void WriteNonFiniteFloatValue(Utf8JsonWriter writer, float number)
        {
            if (double.IsNaN(number))
                writer.WriteStringValue("NaN"u8);
            else if (double.IsPositiveInfinity(number))
                writer.WriteStringValue("Infinity"u8);
            else if (double.IsNegativeInfinity(number))
                writer.WriteStringValue("-Infinity"u8);
            else
                throw new NotSupportedException();
        }


        /// <summary> Deserializes JSON primitive values to dotnet primitives, mimicking the Newtonsoft.Json behavior to some degree </summary>
        public static object? DeserializeObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString()!;
            }
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }
            if (reader.TokenType == JsonTokenType.True)
            {
                return BoxingUtils.True;
            }
            if (reader.TokenType == JsonTokenType.False)
            {
                return BoxingUtils.False;
            }
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            return JsonElement.ParseValue(ref reader);
        }

        public static T Deserialize<T>(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (typeof(T) == typeof(object))
            {
                return (T)DeserializeObject(ref reader, options)!;
            }
            return JsonSerializer.Deserialize<T>(ref reader, options)!;
        }
    }

#if NET6_0_OR_GREATER
    public class HalfJsonConverter : JsonConverter<Half>
    {
        public override Half Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => (Half)reader.GetFloat32Value();

        public override void Write(Utf8JsonWriter writer, Half value, JsonSerializerOptions options) => SystemTextJsonUtils.WriteFloatValue(writer, (float)value);
    }
#endif
}
