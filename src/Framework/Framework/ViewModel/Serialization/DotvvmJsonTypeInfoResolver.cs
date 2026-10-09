using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary> Leaves polymorphic dispatch to the DotVVM converter, retaining standard contracts for other converters. </summary>
    public sealed class DotvvmJsonTypeInfoResolver(IJsonTypeInfoResolver inner) : IJsonTypeInfoResolver
    {
        public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var info = inner.GetTypeInfo(type, options);
            if (info?.Converter is IDotvvmJsonConverter)
                info.PolymorphismOptions = null;
            return info;
        }
    }
}
