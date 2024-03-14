using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

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


        /// <summary> Deserializes JSON primitive values to dotnet primitives, mimicking the Newtonsoft.Json behavior to some degree </summary>
        public static object DeserializeObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
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
                return true;
            }
            if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null!;
            }
            return JsonSerializer.Deserialize<object>(ref reader, options)!;
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
}
