using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using DotVVM.Framework.Routing;

namespace DotVVM.Framework.Utils
{
    static class SystemTextJsonHacks
    {
        public static void Populate<T>(T obj, string input, JsonSerializerOptions options)
            where T: class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            options = new JsonSerializerOptions(options);
            options.TypeInfoResolver = new Resolver<T>(options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver(), obj);

            var length = StringUtils.Utf8.GetByteCount(input) + 6;
            var bytes = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                """{"X":"""u8.CopyTo(bytes.AsSpan().Slice(0, 5));
                StringUtils.Utf8Encode(input.AsSpan(), bytes.AsSpan(5));
                bytes[length - 1] = (byte)'}';
                var reader = new Utf8JsonReader(bytes.AsSpan().Slice(0, length));
                var result = JsonSerializer.Deserialize<PopulateClass<T>>(ref reader, options)?.X;
                if (!object.ReferenceEquals(result, obj))
                {
                    throw new InvalidOperationException("The object was not populated correctly.");
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }

        class PopulateClass<T>
        {
            [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
            public T X { get; init; } = default!;
        }

        class Resolver<T>: IJsonTypeInfoResolver
        {
            private readonly IJsonTypeInfoResolver inner;
            private readonly T populateInstance;

            public Resolver(IJsonTypeInfoResolver inner, T populateInstance)
            {
                this.inner = inner;
                this.populateInstance = populateInstance;
            }
            public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
            {
                var info = inner.GetTypeInfo(type, options);
                if (info?.Type == typeof(PopulateClass<T>))
                {
                    info.CreateObject = () => {
                        return new PopulateClass<T> { X = populateInstance };
                    };
                }
                return info;
            }
        }
    }
}
