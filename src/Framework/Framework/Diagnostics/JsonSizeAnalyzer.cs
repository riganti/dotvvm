using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using FastExpressionCompiler;

namespace DotVVM.Framework.Diagnostics
{
    /// <summary> Computes the inclusive and exclusive size of each JSON property. </summary>
    public class JsonSizeAnalyzer
    {
        readonly IViewModelSerializationMapper viewModelMapper;

        public JsonSizeAnalyzer(IViewModelSerializationMapper viewModelMapper)
        {
            this.viewModelMapper = viewModelMapper;
        }
        /// <summary> Computes the inclusive and exclusive size of each JSON property. </summary>
        public JsonSizeProfile Analyze(ReadOnlySpan<byte> json, Type? rootViewModelType)
        {
            var reader = new Utf8JsonReader(json);
            return Analyze(ref reader, rootViewModelType);
        }

        /// <summary> Computes the inclusive and exclusive size of each JSON property. </summary>
        public JsonSizeProfile Analyze(ref Utf8JsonReader json, Type? rootViewModelType)
        {
            Dictionary<string, ClassSizeProfile> results = new();
            // returns the length of the token. Recursively calls itself for arrays and objects.
            AtomicSizeProfile analyzeNode(ref Utf8JsonReader json, Type? type)
            {
                switch (json.TokenType)
                {
                    case JsonTokenType.StartObject:
                        return new (InclusiveSize: analyzeObject(ref json, type), ExclusiveSize: 2);
                    case JsonTokenType.StartArray: {
                        json.Read();
                        var elementType = type?.GetEnumerableType();
                        var r = new AtomicSizeProfile(0);
                        while (json.TokenType != JsonTokenType.EndArray)
                        {
                            r = new AtomicSizeProfile(r.InclusiveSize + analyzeNode(ref json, elementType).InclusiveSize);
                            json.Read();
                        }
                        if (json.TokenType != JsonTokenType.EndArray)
                            throw new JsonException($"Expected EndArray, found {json.TokenType}.");
                        return r;
                    }
                    case JsonTokenType.String:
                        return new (json.GetValueLength() + 2);
                    case JsonTokenType.Number:
                        return new (json.GetValueLength());
                    case JsonTokenType.True:
                        return new(4);
                    case JsonTokenType.False:
                        return new(5);
                    case JsonTokenType.Null:
                        return new(4);
                    default: {
                        Debug.Assert(false, $"Unexpected token type {json.TokenType}");
                        var start = json.TokenStartIndex;
                        json.Skip();
                        return new((int)(json.BytesConsumed - start));
                    }
                }
            }
            int analyzeObject(ref Utf8JsonReader json, Type? type)
            {
                var typeMap = type is null ? null : viewModelMapper.GetMap(type);
                var typeName = type?.ToCode(stripNamespace: true) ?? "UnknownType";

                var props = new Dictionary<string, AtomicSizeProfile>();

                var startIndex = json.TokenStartIndex;
                var exclusiveSize = 2;

                json.AssertRead(JsonTokenType.StartObject);
                while (json.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = json.GetString().NotNull();
                    var propNameLength = json.GetValueLength();
                    json.Read();
                    if (propName == "$type")
                    {
                        typeMap = viewModelMapper.GetMapByTypeId(json.GetString().NotNull("$type"));
                        type = typeMap.Type;
                        typeName = typeMap.Type.ToCode(stripNamespace: true);
                    }

                    var propertyMap = typeMap?.Properties.FirstOrDefault(p => p.Name == propName);

                    var propSize = analyzeNode(ref json, propertyMap?.Type);
                    props[propertyMap?.PropertyInfo?.Name ?? propName] = propSize + new AtomicSizeProfile(propNameLength + 4); // 2 for the quotes, 1 for :, 1 for ,
                    exclusiveSize += propNameLength + 4;
                    json.Read();
                }
                if (json.TokenType != JsonTokenType.EndObject)
                    throw new JsonException($"Expected EndObject but found {json.TokenType}");

                var inclusiveSize = (int)(json.BytesConsumed - startIndex);
                var classSize = new ClassSizeProfile(new AtomicSizeProfile(inclusiveSize, exclusiveSize), props);
                if (results.TryGetValue(typeName, out var existing))
                {
                    results[typeName] = existing + classSize;
                }
                else
                {
                    results[typeName] = classSize;
                }
                return inclusiveSize;
            }
            
            if (json.TokenType == JsonTokenType.None)
                json.AssertRead(JsonTokenType.None);

            var totalSize = analyzeObject(ref json, rootViewModelType);
            return new JsonSizeProfile(results, totalSize);
        }


        public record JsonSizeProfile(
            Dictionary<string, ClassSizeProfile> Classes,
            int TotalSize
        );
        public record ClassSizeProfile(
            AtomicSizeProfile Size,
            Dictionary<string, AtomicSizeProfile> Properties,
            int Count = 1
        ) {
            public static ClassSizeProfile operator +(ClassSizeProfile a, ClassSizeProfile b)
            {
                var props = new Dictionary<string, AtomicSizeProfile>(a.Properties);
                foreach (var prop in b.Properties)
                {
                    props[prop.Key] = props.GetValueOrDefault(prop.Key) + prop.Value;
                }
                return new(
                    a.Size + b.Size,
                    props,
                    a.Count + b.Count
                );
            }
        }
        public record struct AtomicSizeProfile(
            int InclusiveSize,
            int ExclusiveSize
        ) {
            public AtomicSizeProfile(int exclusiveSize): this(exclusiveSize, exclusiveSize) { }

            public static AtomicSizeProfile operator +(AtomicSizeProfile a, AtomicSizeProfile b) => new AtomicSizeProfile(a.InclusiveSize + b.InclusiveSize, a.ExclusiveSize + b.ExclusiveSize);
            public static AtomicSizeProfile operator +(AtomicSizeProfile a, int c) => new AtomicSizeProfile(a.InclusiveSize + c, a.ExclusiveSize + c);
        }
    }
}
