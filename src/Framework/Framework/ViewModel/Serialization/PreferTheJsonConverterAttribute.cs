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
            if (attr.CreateConverter(typeToConvert) is {} converter)
                return converter;
            if (attr.ConverterType is null)
                throw new Exception($"Type {typeToConvert.ToCode()} has a [JsonConverter] attribute, but its CreateConverter returns null sometimes?");
            return (JsonConverter)Activator.CreateInstance(attr.ConverterType)!;
        }
    }
}

