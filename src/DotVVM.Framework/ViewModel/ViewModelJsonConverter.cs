using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.ViewModel
{
    /// <summary>
    /// A JSON.NET converter that handles special features of DotVVM ViewModel serialization.
    /// </summary>
    public class ViewModelJsonConverter : JsonConverter
    {
        private static readonly Type[] primitiveTypes = {
            typeof(string), typeof(bool), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(Guid),
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal)
        };

        private static readonly ViewModelSerializationMapper viewModelSerializationMapper = new ViewModelSerializationMapper();
        private static readonly ConcurrentDictionary<Type, ViewModelSerializationMap> serializationMapCache = new ConcurrentDictionary<Type, ViewModelSerializationMap>();

        public ViewModelJsonConverter(bool isPostback, JObject encryptedValues = null)
        {
            IsPostback = isPostback;
            EncryptedValues = encryptedValues ?? new JObject();
            evReader = EncryptedValuesReader.FromObject(EncryptedValues);
            evWriter = new EncryptedValuesWriter(EncryptedValues.CreateWriter());
        }

        public JObject EncryptedValues { get; }
        private EncryptedValuesReader evReader;
        private EncryptedValuesWriter evWriter;


        public HashSet<ViewModelSerializationMap> UsedSerializationMaps { get; set; }
        public bool IsPostback { get; private set; }

        /// <summary>
        /// Gets the serialization map for specified type.
        /// </summary>
        public static ViewModelSerializationMap GetSerializationMapForType(Type type)
        {
            return serializationMapCache.GetOrAdd(type, viewModelSerializationMapper.CreateMap);
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return !IsEnumerable(objectType) && IsComplexType(objectType);
        }



        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // handle null keyword
            if (reader.TokenType == JsonToken.Null)
            {
                if (objectType.IsValueType)
                    throw new InvalidOperationException(string.Format("Recieved NULL for value type. Path: " + reader.Path));

                return null;
            }

            // deserialize
            var serializationMap = GetSerializationMapForType(objectType);
            var instance = serializationMap.ConstructorFactory();
            serializationMap.ReaderFactory(JObject.Load(reader), serializer, instance, evReader);
            return instance;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var serializationMap = GetSerializationMapForType(value.GetType());
            UsedSerializationMaps.Add(serializationMap);
            serializationMap.WriterFactory(writer, value, serializer, evWriter, IsPostback);
        }

        /// <summary>
        /// Populates the specified JObject.
        /// </summary>
        public virtual void Populate(JObject jobj, JsonSerializer serializer, object value)
        {
            var serializationMap = GetSerializationMapForType(value.GetType());
            serializationMap.ReaderFactory(jobj, serializer, value, evReader);
        }


        public static bool IsEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsPrimitiveType(Type type)
        {
            return primitiveTypes.Contains(type);
        }

        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsEnum(Type type)
        {
            return type.IsEnum;
        }

        public static bool IsComplexType(Type type)
        {
            return !IsPrimitiveType(type) && !IsEnum(type) && !IsNullableType(type) && type != typeof(object);
        }
    }
}
