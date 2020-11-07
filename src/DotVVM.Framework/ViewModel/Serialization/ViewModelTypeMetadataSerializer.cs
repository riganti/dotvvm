using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.ViewModel.Serialization
{

    public class ViewModelTypeMetadataSerializer
    {
        private static readonly HashSet<Type> supportedPrimitiveTypes = new HashSet<Type>()
        {
            typeof(Boolean),
            typeof(Byte),
            typeof(SByte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(String),
            typeof(Char),
            typeof(Guid),
            typeof(DateTime),
            typeof(TimeSpan)
        };

        private static readonly ConcurrentDictionary<ViewModelSerializationMap, JObject> cachedJson = new ConcurrentDictionary<ViewModelSerializationMap, JObject>();

        public JToken SerializeTypeMetadata(IEnumerable<ViewModelSerializationMap> usedSerializationMaps, ILookup<string, string> ignoredTypes = null)
        {
            var types = new JObject();
            foreach (var map in usedSerializationMaps)
            {
                var type = GetComplexTypeName(map.Type);
                if (ignoredTypes?.Contains(type) != true)
                {
                    types[type] = GetTypeMetadataCopy(map);
                }
            }
            return types;
        }

        private JObject GetTypeMetadataCopy(ViewModelSerializationMap map)
        {
            var obj = cachedJson.GetOrAdd(map, BuildTypeMetadata);
            return (JObject)obj.DeepClone();
        }

        internal JObject BuildTypeMetadata(ViewModelSerializationMap map)
        {
            var type = new JObject();

            foreach (var property in map.Properties)
            {
                var prop = new JObject();

                prop["type"] = GetTypeIdentifier(property.Type);

                if (property.TransferToServerOnlyInPath)
                {
                    prop["post"] = "pathOnly";
                }
                if (!property.TransferToServer)
                {
                    prop["post"] = "no";
                }

                if (!property.TransferAfterPostback)
                {
                    prop["update"] = property.TransferFirstRequest ? "firstRequest" : "no";
                }

                if (property.ValidationRules.Any() && property.ClientValidationRules.Any())
                {
                    prop["validationRules"] = JToken.FromObject(property.ClientValidationRules);
                }

                type[property.Name] = prop;
            }

            return type;
        }

        internal JToken GetTypeIdentifier(Type type)
        {
            if (supportedPrimitiveTypes.Contains(type))
            {
                return GetPrimitiveTypeName(type);
            }
            else if (type.IsEnum)
            {
                return GetEnumTypeIdentifier(type);
            }
            else if (ReflectionUtils.IsNullable(type))
            {
                return GetNullableTypeIdentifier(type);
            }
            else if (ReflectionUtils.IsCollection(type))
            {
                return new JArray(GetTypeIdentifier(ReflectionUtils.GetEnumerableType(type)));
            }
            else
            {
                return GetComplexTypeName(type);
            }
        }

        private JToken GetNullableTypeIdentifier(Type type)
        {
            var n = new JObject();
            n["type"] = "nullable";
            n["inner"] = GetTypeIdentifier(ReflectionUtils.UnwrapNullableType(type));
            return n;
        }

        private JToken GetEnumTypeIdentifier(Type type)
        {
            var e = new JObject();
            e["type"] = "enum";

            var values = new JObject();
            foreach (var v in Enum.GetValues(type))
            {
                values[v.ToString()] = JToken.FromObject(ReflectionUtils.ConvertValue(v, Enum.GetUnderlyingType(type)));
            }
            e["values"] = values;

            return e;
        }

        private string GetComplexTypeName(Type type) => type.GetTypeHash();

        private string GetPrimitiveTypeName(Type type) => type.Name.ToString();

    }

}
