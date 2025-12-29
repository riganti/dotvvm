using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public sealed class PreferTheJsonConverterAttribute : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsDefined(typeof(JsonConverterAttribute), inherit: false))
                return false;
            var attr = typeToConvert.GetCustomAttribute<JsonConverterAttribute>(inherit: false)!;
            return attr.ConverterType is {} || attr.CreateConverter(typeToConvert) is {};
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var attr = typeToConvert.GetCustomAttribute<JsonConverterAttribute>(inherit: false)!;
            var converter = attr.CreateConverter(typeToConvert);
            if (converter is null)
            {
                if (attr.ConverterType is null)
                    throw new Exception($"Type {typeToConvert.ToCode()} has a [JsonConverter] attribute, but its CreateConverter returns null sometimes?");
                converter = (JsonConverter)Activator.CreateInstance(attr.ConverterType)!;
            }

            if (converter is JsonConverterFactory factory)
                return factory.CreateConverter(typeToConvert, options);
            return converter;
        }
    }
}

