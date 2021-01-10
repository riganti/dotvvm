using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using DotVVM.Framework.Utils;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.ViewModel.Serialization
{

    public class ViewModelTypeMetadataSerializer : IViewModelTypeMetadataSerializer
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

        private static readonly ConcurrentDictionary<ViewModelSerializationMapWithCulture, ObjectMetadataWithDependencies> cachedObjectMetadata = new ConcurrentDictionary<ViewModelSerializationMapWithCulture, ObjectMetadataWithDependencies>();
        private static readonly ConcurrentDictionary<Type, JObject> cachedEnumMetadata = new ConcurrentDictionary<Type, JObject>();

        public JToken SerializeTypeMetadata(IEnumerable<ViewModelSerializationMap> usedSerializationMaps, ISet<string> ignoredTypes = null)
        {
            var dependentEnumTypes = new HashSet<Type>();

            // serialize object types
            var types = new JObject();
            foreach (var map in usedSerializationMaps)
            {
                var typeId = GetComplexTypeName(map.Type);
                if (ignoredTypes?.Contains(typeId) != true)
                {
                    var metadata = GetObjectTypeMetadataCopy(map);
                    types[typeId] = metadata.Metadata;
                    dependentEnumTypes.UnionWith(metadata.DependentEnumTypes);
                }
            }

            // add enum types
            foreach (var type in dependentEnumTypes)
            {
                var typeId = GetEnumTypeName(type);
                if (ignoredTypes?.Contains(typeId) != true)
                {
                    types[typeId] = GetEnumTypeMetadataCopy(type);
                }
            }
            
            return types;
        }

        private JObject GetEnumTypeMetadataCopy(Type type)
        {
            var metadata = cachedEnumMetadata.GetOrAdd(type, BuildEnumTypeMetadata);
            return (JObject)metadata.DeepClone();
        }

        private ObjectMetadataWithDependencies GetObjectTypeMetadataCopy(ViewModelSerializationMap map)
        {
            var key = new ViewModelSerializationMapWithCulture(map, CultureInfo.CurrentUICulture.Name);
            var obj = cachedObjectMetadata.GetOrAdd(key, BuildObjectTypeMetadata(map));
            return new ObjectMetadataWithDependencies((JObject)obj.Metadata.DeepClone(), obj.DependentEnumTypes);
        }

        private ObjectMetadataWithDependencies BuildObjectTypeMetadata(ViewModelSerializationMap map)
        {
            var dependentEnumTypes = new HashSet<Type>();
            
            var type = new JObject();
            type["type"] = "object";

            var properties = new JObject();
            foreach (var property in map.Properties.Where(p => p.IsAvailableOnClient()))
            {
                var prop = new JObject();

                prop["type"] = GetTypeIdentifier(property.Type, dependentEnumTypes);

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

                if (property.ClientExtenders.Any())
                {
                    prop["clientExtenders"] = JToken.FromObject(property.ClientExtenders);
                }

                properties[property.Name] = prop;
            }

            type["properties"] = properties;

            return new ObjectMetadataWithDependencies(type, dependentEnumTypes);
        }

        internal JToken GetTypeIdentifier(Type type, HashSet<Type> dependentEnumTypes)
        {
            if (supportedPrimitiveTypes.Contains(type))
            {
                return GetPrimitiveTypeName(type);
            }
            else if (type == typeof(object))
            {
                return new JObject(new JProperty("type", "dynamic"));
            }
            else if (type.IsEnum)
            {
                dependentEnumTypes.Add(type);
                return GetEnumTypeName(type);
            }
            else if (ReflectionUtils.IsNullable(type))
            {
                return GetNullableTypeIdentifier(type, dependentEnumTypes);
            }
            else if (ReflectionUtils.IsCollection(type))
            {
                return new JArray(GetTypeIdentifier(ReflectionUtils.GetEnumerableType(type), dependentEnumTypes));
            }
            else
            {
                return GetComplexTypeName(type);
            }
        }

        private JToken GetNullableTypeIdentifier(Type type, HashSet<Type> dependentEnumTypes)
        {
            var n = new JObject();
            n["type"] = "nullable";
            n["inner"] = GetTypeIdentifier(ReflectionUtils.UnwrapNullableType(type), dependentEnumTypes);
            return n;
        }

        private JObject BuildEnumTypeMetadata(Type type)
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

        private string GetEnumTypeName(Type type) => type.GetTypeHash();

        private string GetPrimitiveTypeName(Type type) => type.Name.ToString();


        readonly struct ObjectMetadataWithDependencies
        {
            public JObject Metadata { get; }

            public HashSet<Type> DependentEnumTypes { get; }

            public ObjectMetadataWithDependencies(JObject metadata, HashSet<Type> dependentEnumTypes)
            {
                Metadata = metadata;
                DependentEnumTypes = dependentEnumTypes;
            }
        }

        readonly struct ViewModelSerializationMapWithCulture : IEquatable<ViewModelSerializationMapWithCulture>
        {
            public ViewModelSerializationMap Map { get; }
            
            public string CultureName { get; }

            public ViewModelSerializationMapWithCulture(ViewModelSerializationMap map, string cultureName)
            {
                Map = map;
                CultureName = cultureName;
            }

            public override bool Equals(object obj) => obj is ViewModelSerializationMapWithCulture culture && Equals(culture);
            public bool Equals(ViewModelSerializationMapWithCulture other) => EqualityComparer<ViewModelSerializationMap>.Default.Equals(Map, other.Map) && CultureName == other.CultureName;

            public override int GetHashCode()
            {
                var hashCode = 692496131;
                hashCode = hashCode * -1521134295 + EqualityComparer<ViewModelSerializationMap>.Default.GetHashCode(Map);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CultureName);
                return hashCode;
            }

            public static bool operator ==(ViewModelSerializationMapWithCulture left, ViewModelSerializationMapWithCulture right) => left.Equals(right);
            public static bool operator !=(ViewModelSerializationMapWithCulture left, ViewModelSerializationMapWithCulture right) => !(left == right);
        }
    }

}
