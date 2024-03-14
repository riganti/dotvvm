using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;

namespace DotVVM.Framework.ViewModel.Serialization
{

    public class ViewModelTypeMetadataSerializer : IViewModelTypeMetadataSerializer
    {
        private readonly IViewModelSerializationMapper viewModelSerializationMapper;
        private readonly bool debug;
        private readonly bool serializeValidationRules;
        private readonly ConcurrentDictionary<ViewModelSerializationMapWithCulture, ObjectMetadataWithDependencies> cachedObjectMetadata = new ConcurrentDictionary<ViewModelSerializationMapWithCulture, ObjectMetadataWithDependencies>();
        private readonly ConcurrentDictionary<Type, byte[]> cachedEnumMetadata = new();

        public ViewModelTypeMetadataSerializer(IViewModelSerializationMapper viewModelSerializationMapper, DotvvmConfiguration? config = null)
        {
            this.viewModelSerializationMapper = viewModelSerializationMapper;
            this.debug = config != null && config.Debug;
            this.serializeValidationRules = config is null || config.ClientSideValidation;

            HotReloadMetadataUpdateHandler.TypeMetadataSerializer.Add(new(this));
        }

        public void SerializeTypeMetadata(IEnumerable<ViewModelSerializationMap> usedSerializationMaps, Utf8JsonWriter json, ReadOnlySpan<byte> propertyName, ISet<string>? ignoredTypes = null)
        {
            var dependentEnumTypes = new HashSet<Type>();

            // serialize object types
            var queue = new Queue<ViewModelSerializationMap>();
            var visitedTypes = new HashSet<Type>();
            foreach (var map in usedSerializationMaps)
            {
                visitedTypes.Add(map.Type);
                if (ignoredTypes?.Contains(map.ClientTypeId) != true)
                    queue.Enqueue(map);
            }
            if (queue.Count == 0)
                return;

            json.WriteStartObject(propertyName);

            while (queue.Count > 0)
            {
                var map = queue.Dequeue();
                var typeId = map.ClientTypeId;

                json.WritePropertyName(typeId);
                var metadata = GetObjectTypeMetadataCached(map);
                json.WriteRawValue(metadata.MetadataJson, skipInputValidation: true);

                dependentEnumTypes.UnionWith(metadata.DependentEnumTypes);

                foreach (var dependentType in metadata.DependentObjectTypes)
                {
                    if (!visitedTypes.Contains(dependentType))
                    {
                        visitedTypes.Add(dependentType);
                        if (ignoredTypes?.Contains(map.ClientTypeId) != true)
                            queue.Enqueue(viewModelSerializationMapper.GetMap(dependentType));
                    }
                }
            }

            // add enum types
            foreach (var type in dependentEnumTypes)
            {
                var typeId = GetEnumTypeName(type);
                if (ignoredTypes?.Contains(typeId) != true)
                {
                    json.WritePropertyName(typeId);
                    json.WriteRawValue(GetEnumTypeMetadataCached(type), skipInputValidation: true);
                }
            }
            json.WriteEndObject();
        }

        private byte[] GetEnumTypeMetadataCached(Type type)
        {
            return cachedEnumMetadata.GetOrAdd(type, BuildEnumTypeMetadata);
        }

        private ObjectMetadataWithDependencies GetObjectTypeMetadataCached(ViewModelSerializationMap map)
        {
            var key = new ViewModelSerializationMapWithCulture(map, CultureInfo.CurrentUICulture.Name);
            return cachedObjectMetadata.GetOrAdd(key, _ => BuildObjectTypeMetadata(map));
        }

        private ObjectMetadataWithDependencies BuildObjectTypeMetadata(ViewModelSerializationMap map)
        {
            var dependentEnumTypes = new HashSet<Type>();
            var dependentObjectTypes = new HashSet<Type>();

            var buffer = new MemoryStream();
            var json = new Utf8JsonWriter(buffer, new JsonWriterOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            json.WriteStartObject();
            json.WriteString("type"u8, "object"u8);
            if (debug)
            {
                json.WriteString("debugName"u8, map.Type.ToCode(stripNamespace: true));
            }

            json.WriteStartObject("properties"u8);
            foreach (var property in map.Properties.Where(p => p.IsAvailableOnClient()))
            {
                json.WriteStartObject(property.Name);

                json.WritePropertyName("type"u8);
                WriteTypeIdentifier(json, property.Type, dependentObjectTypes, dependentEnumTypes);

                if (debug && property.Name != property.PropertyInfo.Name)
                {
                    json.WriteString("debugName"u8, property.PropertyInfo.Name);
                }

                if (property.TransferToServerOnlyInPath)
                {
                    json.WriteString("post"u8, "pathOnly"u8);
                }
                else if (!property.TransferToServer)
                {
                    json.WriteString("post"u8, "no"u8);
                }

                if (!property.TransferAfterPostback)
                {
                    json.WriteString("update"u8, "no"u8);
                }

                if (serializeValidationRules && property.ValidationRules.Any() && property.ClientValidationRules.Any())
                {
                    json.WritePropertyName("validationRules"u8);
                    JsonSerializer.Serialize(json, property.ClientValidationRules, DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe);
                }

                if (property.ClientExtenders.Any())
                {
                    json.WritePropertyName("clientExtenders"u8);
                    JsonSerializer.Serialize(json, property.ClientExtenders, DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe);
                }

                json.WriteEndObject();
            }

            json.WriteEndObject();
            json.WriteEndObject();
            json.Dispose();

            return new ObjectMetadataWithDependencies(buffer.ToArray(), dependentObjectTypes, dependentEnumTypes);
        }

        internal void WriteTypeIdentifier(Utf8JsonWriter json, Type type, HashSet<Type> dependentObjectTypes, HashSet<Type> dependentEnumTypes)
        {
            if (type.IsEnum)
            {
                dependentEnumTypes.Add(type);
                json.WriteStringValue(GetEnumTypeName(type));
            }
            else if (ReflectionUtils.IsNullable(type))
            {
                json.WriteStartObject();
                json.WriteString("type"u8, "nullable"u8);
                json.WritePropertyName("inner");
                WriteTypeIdentifier(json, ReflectionUtils.UnwrapNullableType(type), dependentObjectTypes, dependentEnumTypes);

                json.WriteEndObject();
            }
            else if (ReflectionUtils.IsPrimitiveType(type))        // we intentionally detect this after handling enums and nullable types
            {
                if (ReflectionUtils.TryGetCustomPrimitiveTypeRegistration(type) is {})
                    json.WriteStringValue(GetPrimitiveTypeName(typeof(string)));
                else
                    json.WriteStringValue(GetPrimitiveTypeName(type));
            }
            else if (type == typeof(object))
            {
                json.WriteStartObject();
                json.WriteString("type"u8, "dynamic"u8);
                json.WriteEndObject();
            }
            else if (type.IsGenericType && ReflectionUtils.ImplementsGenericDefinition(type, typeof(IDictionary<,>)))
            {
                json.WriteStartArray();
                var attrs = type.GetGenericArguments();
                var keyValuePair = typeof(KeyValuePair<,>).MakeGenericType(attrs);
                WriteTypeIdentifier(json, keyValuePair, dependentObjectTypes, dependentEnumTypes);
                json.WriteEndArray();
            }
            else if (ReflectionUtils.IsCollection(type))
            {
                json.WriteStartArray();
                WriteTypeIdentifier(json, ReflectionUtils.GetEnumerableType(type)!, dependentObjectTypes, dependentEnumTypes);
                json.WriteEndArray();
            }
            else
            {
                dependentObjectTypes.Add(type);
                json.WriteStringValue(GetComplexTypeName(type));
            }
        }

        private byte[] BuildEnumTypeMetadata(Type type)
        {
            var buffer = new MemoryStream();
            var json = new Utf8JsonWriter(buffer, new JsonWriterOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            json.WriteStartObject();
            json.WriteString("type"u8, "enum"u8);
            if (type.IsDefined(typeof(FlagsAttribute)))
            {
                json.WriteBoolean("isFlags"u8, true);
            }

            if (debug)
            {
                json.WriteString("debugName"u8, type.ToCode(stripNamespace: true));
            }

            // order of enum values is important on the client (for Flags enum coercion)
            // Enum.GetNames and Enum.GetValues return the enums in ascending order (in unsigned value)
            var underlyingType = Enum.GetUnderlyingType(type);
            var enumValues = Enum.GetNames(type)
                .Zip(
                    Enum.GetValues(type).Cast<object>().Select(c => Convert.ChangeType(c, underlyingType)),
                    (name, value) => new {
                    Name = name,
                    Value = unchecked((long)(dynamic)value)
                });

            json.WriteStartObject("values"u8);
            foreach (var v in enumValues)
            {
                json.WriteNumber(
                    ReflectionUtils.ToEnumString(type, v.Name),
                    v.Value
                );
            }
            json.WriteEndObject();
            json.WriteEndObject();
            json.Dispose();

            return buffer.ToArray();
        }

        private string GetComplexTypeName(Type type) => type.GetTypeHash();

        private string GetEnumTypeName(Type type) => type.GetTypeHash();

        private string GetPrimitiveTypeName(Type type) => type.Name.ToString();


        readonly struct ObjectMetadataWithDependencies
        {
            public byte[] MetadataJson { get; }

            public HashSet<Type> DependentObjectTypes { get; }

            public HashSet<Type> DependentEnumTypes { get; }

            public ObjectMetadataWithDependencies(byte[] metadataJson, HashSet<Type> dependentObjectTypes, HashSet<Type> dependentEnumTypes)
            {
                MetadataJson = metadataJson;
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
        internal void ClearCaches(Type[] types)
        {
            foreach (var t in types)
                cachedEnumMetadata.TryRemove(t, out _);
            
            // metadata does not have to be cleared, since it will get regenerated in the ViewModelSerializationMapper
        }
    }

}
