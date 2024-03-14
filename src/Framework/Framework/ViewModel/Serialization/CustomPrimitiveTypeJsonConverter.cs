using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DotvvmCustomPrimitiveTypeConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            ReflectionUtils.IsCustomPrimitiveType(typeToConvert);
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter)Activator.CreateInstance(typeof(InnerConverter<>).MakeGenericType(typeToConvert))!;

        class InnerConverter<T>: JsonConverter<T> where T: IDotvvmPrimitiveType
        {
            private CustomPrimitiveTypeRegistration registration = ReflectionUtils.TryGetCustomPrimitiveTypeRegistration(typeof(T))!;
            // TODO: make this converter factory?
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType is JsonTokenType.String
                    or JsonTokenType.True
                    or JsonTokenType.False
                    or JsonTokenType.Number)
                {
                    // TODO: utf8 parsing?
                    var registration = ReflectionUtils.TryGetCustomPrimitiveTypeRegistration(typeToConvert)!;
                    var str = reader.TokenType is JsonTokenType.String ? reader.GetString() :
                              reader.HasValueSequence ? StringUtils.Utf8Decode(reader.ValueSequence.ToArray()) :
                              StringUtils.Utf8Decode(reader.ValueSpan);
                    var parseResult = registration.TryParseMethod(str!);
                    if (!parseResult.Successful)
                    {
                        throw new Exception($"The value '{str}' cannot be deserialized as {typeToConvert} because its TryParse method wasn't able to parse the value!");
                    }
                    return (T)parseResult.Result!;
                }
                else if (reader.TokenType == JsonTokenType.Null)
                {
                    return default;
                }
                else
                {
                    throw new Exception($"Token {reader.TokenType} cannot be deserialized as {typeToConvert}! Primitive value in JSON was expected.");
                }
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(registration.ToStringMethod(value));
            }
        }
    }
}
