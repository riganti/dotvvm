using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.ViewModel.Serialization
{

    public class ViewModelTypeMetadataSerializer : IViewModelTypeMetadataSerializer
    {
        private readonly IViewModelSerializationMapper viewModelSerializationMapper;
        private readonly bool debug;
        private static readonly ConcurrentDictionary<ViewModelSerializationMapWithCulture, ObjectMetadataWithDependencies> cachedObjectMetadata = new ConcurrentDictionary<ViewModelSerializationMapWithCulture, ObjectMetadataWithDependencies>();
        private static readonly ConcurrentDictionary<Type, JObject> cachedEnumMetadata = new ConcurrentDictionary<Type, JObject>();

        public ViewModelTypeMetadataSerializer(IViewModelSerializationMapper viewModelSerializationMapper, DotvvmConfiguration? config = null)
        {
            this.viewModelSerializationMapper = viewModelSerializationMapper;
            this.debug = config != null && config.Debug;
        }

        public JObject SerializeTypeMetadata(IEnumerable<ViewModelSerializationMap> usedSerializationMaps, ISet<string>? ignoredTypes = null)
        {
            var dependentEnumTypes = new HashSet<Type>();
            var resultJson = new JObject();

            // serialize object types
            var queue = new Queue<ViewModelSerializationMap>();
            var visitedTypes = new HashSet<Type>();
            foreach (var map in usedSerializationMaps)
            {
                queue.Enqueue(map);
                visitedTypes.Add(map.Type);
            }

            while (queue.Count > 0)
            {
                var map = queue.Dequeue();
                var typeId = GetComplexTypeName(map.Type);

                if (ignoredTypes?.Contains(typeId) != true)
                {
                    var metadata = GetObjectTypeMetadataCopy(map);
                    resultJson[typeId] = metadata.Metadata;

                    dependentEnumTypes.UnionWith(metadata.DependentEnumTypes);

                    foreach (var dependentType in metadata.DependentObjectTypes)
                    {
                        if (!visitedTypes.Contains(dependentType))
                        {
                            visitedTypes.Add(dependentType);
                            queue.Enqueue(viewModelSerializationMapper.GetMap(dependentType));
                        }
                    }
                }
            }

            // add enum types
            foreach (var type in dependentEnumTypes)
            {
                var typeId = GetEnumTypeName(type);
                if (ignoredTypes?.Contains(typeId) != true)
                {
                    resultJson[typeId] = GetEnumTypeMetadataCopy(type);
                }
            }

            return resultJson;
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
            return new ObjectMetadataWithDependencies((JObject)obj.Metadata.DeepClone(), obj.DependentObjectTypes, obj.DependentEnumTypes);
        }

        private ObjectMetadataWithDependencies BuildObjectTypeMetadata(ViewModelSerializationMap map)
        {
            var dependentEnumTypes = new HashSet<Type>();
            var dependentObjectTypes = new HashSet<Type>();

            var type = new JObject();
            type["type"] = "object";
            if (debug)
            {
                type["debugName"] = map.Type.ToCode(stripNamespace: true);
            }

            var properties = new JObject();
            foreach (var property in map.Properties.Where(p => p.IsAvailableOnClient()))
            {
                var prop = new JObject();

                prop["type"] = GetTypeIdentifier(property.Type, dependentObjectTypes, dependentEnumTypes);

                if (debug && property.Name != property.PropertyInfo.Name)
                {
                    prop["debugName"] = property.PropertyInfo.Name;
                }

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
                    prop["update"] = "no";
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

            return new ObjectMetadataWithDependencies(type, dependentObjectTypes, dependentEnumTypes);
        }

        internal JToken GetTypeIdentifier(Type type, HashSet<Type> dependentObjectTypes, HashSet<Type> dependentEnumTypes)
        {
            if (type.IsSerializationSupported(includeNullables: false))
            {
                return GetPrimitiveTypeName(type);
            }
            else if (type == typeof(object))
            {
                return new JObject(new JProperty("type", "dynamic"));
            }
            else if (type.IsGenericType && ReflectionUtils.ImplementsGenericDefinition(type, typeof(IDictionary<,>)))
            {
                var attrs = type.GetGenericArguments();
                var keyValuePair = typeof(KeyValuePair<,>).MakeGenericType(attrs);
                return new JArray(GetTypeIdentifier(keyValuePair, dependentObjectTypes, dependentEnumTypes));
            }
            else if (type.IsEnum)
            {
                dependentEnumTypes.Add(type);
                return GetEnumTypeName(type);
            }
            else if (ReflectionUtils.IsNullable(type))
            {
                return GetNullableTypeIdentifier(type, dependentObjectTypes, dependentEnumTypes);
            }
            else if (ReflectionUtils.IsCollection(type))
            {
                return new JArray(GetTypeIdentifier(ReflectionUtils.GetEnumerableType(type)!, dependentObjectTypes, dependentEnumTypes));
            }
            else
            {
                dependentObjectTypes.Add(type);
                return GetComplexTypeName(type);
            }
        }

        private JToken GetNullableTypeIdentifier(Type type, HashSet<Type> dependentObjectTypes, HashSet<Type> dependentEnumTypes)
        {
            var n = new JObject();
            n["type"] = "nullable";
            n["inner"] = GetTypeIdentifier(ReflectionUtils.UnwrapNullableType(type), dependentObjectTypes, dependentEnumTypes);
            return n;
        }

        private JObject BuildEnumTypeMetadata(Type type)
        {
            var e = new JObject();
            e["type"] = "enum";
            e["isFlags"] = ReflectionUtils.GetCustomAttribute<FlagsAttribute>(type) != null;

            if (debug)
            {
                e["debugName"] = type.ToCode(stripNamespace: true);
            }

            // order of enum values is important on the client (for Flags enum coercion)
            var underlyingType = Enum.GetUnderlyingType(type);
            var enumValues = Enum.GetNames(type)
                .Select(name => new {
                    Name = name,
                    Value = ReflectionUtils.ConvertValue(Enum.Parse(type, name), underlyingType)
                })
                .OrderBy(v => v.Value);

            var values = new JObject();
            foreach (var v in enumValues)
            {
                values.Add(ReflectionUtils.ToEnumString(type, v.Name), JToken.FromObject(v.Value));
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

            public HashSet<Type> DependentObjectTypes { get; }

            public HashSet<Type> DependentEnumTypes { get; }

            public ObjectMetadataWithDependencies(JObject metadata, HashSet<Type> dependentObjectTypes, HashSet<Type> dependentEnumTypes)
            {
                Metadata = metadata;
                DependentObjectTypes = dependentObjectTypes;
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

            public override bool Equals(object? obj) => obj is ViewModelSerializationMapWithCulture culture && Equals(culture);
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

        /// <summary> Clear caches for the specified types </summary>
        internal static void ClearCaches(Type[] types)
        {
            foreach (var t in types)
                cachedEnumMetadata.TryRemove(t, out _);
            
            // metadata does not have to be cleared, since it will get regenerated in the ViewModelSerializationMapper
        }
    }

}
