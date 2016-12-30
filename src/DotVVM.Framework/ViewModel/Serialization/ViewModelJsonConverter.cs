using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.Configuration;
using System.Reflection;

namespace DotVVM.Framework.ViewModel.Serialization
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

        private readonly IViewModelSerializationMapper viewModelSerializationMapper;

        public ViewModelJsonConverter(bool isPostback, IViewModelSerializationMapper viewModelSerializationMapper, JObject encryptedValues = null)
        {
            IsPostback = isPostback;
            EncryptedValues = encryptedValues ?? new JObject();
            evReader = new Lazy<EncryptedValuesReader>(() => {
                evWriter = new Lazy<EncryptedValuesWriter>(() => { throw new Exception("Can't use EncryptedValuesWriter at the same time as EncryptedValuesReader."); });
                return new EncryptedValuesReader(EncryptedValues);
            });
            evWriter = new Lazy<EncryptedValuesWriter>(() => {
                evReader = new Lazy<EncryptedValuesReader>(() => { throw new Exception("Can't use EncryptedValuesReader at the same time as EncryptedValuesWriter."); });
                return new EncryptedValuesWriter(EncryptedValues.CreateWriter());
            });
            this.viewModelSerializationMapper = viewModelSerializationMapper;
        }

        public JObject EncryptedValues { get; }
        private Lazy<EncryptedValuesReader> evReader;
        private Lazy<EncryptedValuesWriter> evWriter;


        public HashSet<ViewModelSerializationMap> UsedSerializationMaps { get; set; }
        public bool IsPostback { get; private set; }

        private ViewModelSerializationMap GetSerializationMapForType(Type type)
        {
            return viewModelSerializationMapper.GetMap(type);
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
                if (objectType.GetTypeInfo().IsValueType)
                    throw new InvalidOperationException(string.Format("Recieved NULL for value type. Path: " + reader.Path));

                return null;
            }

            // deserialize
            var serializationMap = GetSerializationMapForType(objectType);
            var instance = serializationMap.ConstructorFactory();
            serializationMap.ReaderFactory(reader, serializer, instance, evReader.Value);
            return instance;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var serializationMap = GetSerializationMapForType(value.GetType());
            UsedSerializationMaps.Add(serializationMap);
            serializationMap.WriterFactory(writer, value, serializer, evWriter.Value, IsPostback);
        }

        /// <summary>
        /// Populates the specified JObject.
        /// </summary>
        public virtual void Populate(JsonReader reader, JsonSerializer serializer, object value)
        {
            if (reader.TokenType == JsonToken.None) reader.Read();
            var serializationMap = GetSerializationMapForType(value.GetType());
            serializationMap.ReaderFactory(reader, serializer, value, evReader.Value);
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
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsEnum(Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsComplexType(Type type)
        {
            return !IsPrimitiveType(type) && !IsEnum(type) && !IsNullableType(type) && type != typeof(object);
        }
    }
}
