using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using FastExpressionCompiler;
using Newtonsoft.Json.Linq;

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
        public JsonSizeProfile Analyze(JObject json)
        {
            Dictionary<string, ClassSizeProfile> results = new();
            // returns the length of the token. Recursively calls itself for arrays and objects.
            AtomicSizeProfile analyzeToken(JToken token)
            {
                switch (token.Type)
                {
                    case JTokenType.Object:
                        return new (InclusiveSize: analyzeObject((JObject)token), ExclusiveSize: 2);
                    case JTokenType.Array: {
                        var r = new AtomicSizeProfile(0);
                        foreach (var item in (JArray)token)
                        {
                            r += analyzeToken(item);
                        }
                        return r;
                    }
                    case JTokenType.String:
                        return new (((string)token).Length + 2);
                    case JTokenType.Integer:
                        // This should be the same as token.ToString().Length, but I didn't want to allocate the string unnecesarily
                        return new((int)Math.Log10(Math.Abs((long)token) + 1) + 1);
                    case JTokenType.Float:
                        return new(((double)token).ToString().Length);
                    case JTokenType.Boolean:
                        return new(((bool)token) ? 4 : 5);
                    case JTokenType.Null:
                        return new(4);
                    default:
                        Debug.Assert(false);
                        return new(token.ToString().Length);
                }
            }
            int analyzeObject(JObject j)
            {
                var type = ((string?)j.Property("$type")?.Value)?.Apply(viewModelMapper.GetMapByTypeId);

                var typeName = type?.Type.ToCode(stripNamespace: true) ?? "UnknownType";
                var props = new Dictionary<string, AtomicSizeProfile>();

                var totalSize = new AtomicSizeProfile(0);
                foreach (var prop in j.Properties())
                {
                    var propSize = analyzeToken(prop.Value);
                    props[prop.Name] = propSize;

                    totalSize += propSize;
                    totalSize += 4 + prop.Name.Length; // 2 for the quotes, 1 for :, 1 for ,
                }

                var classSize = new ClassSizeProfile(totalSize, props);
                if (results.TryGetValue(typeName, out var existing))
                {
                    results[typeName] = existing + classSize;
                }
                else
                {
                    results[typeName] = classSize;
                }
                return totalSize.InclusiveSize;
            }

            var totalSize = analyzeObject(json);
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
