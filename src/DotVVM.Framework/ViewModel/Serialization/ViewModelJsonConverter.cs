using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.Configuration;
using System.Reflection;
using DotVVM.Framework.Utils;
using System.Security;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// A JSON.NET converter that handles special features of DotVVM ViewModel serialization.
    /// </summary>
    public class ViewModelJsonConverter : JsonConverter
    {
        private readonly IViewModelSerializationMapper viewModelSerializationMapper;

        public ViewModelJsonConverter(bool isPostback, IViewModelSerializationMapper viewModelSerializationMapper, IServiceProvider services, JObject encryptedValues = null)
        {
            Services = services;
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
        public IServiceProvider Services { get; }
        public bool IsPostback { get; private set; }

        private ViewModelSerializationMap GetSerializationMapForType(Type type)
        {
            return viewModelSerializationMapper.GetMap(type);
        }

        public static bool CanConvertType(Type type) =>
            !ReflectionUtils.IsEnumerable(type) &&
            ReflectionUtils.IsComplexType(type) &&
            !ReflectionUtils.IsTupleLike(type) &&
            type != typeof(object);

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return CanConvertType(objectType);
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
                    throw new InvalidOperationException(string.Format("Received NULL for value type. Path: " + reader.Path));

                return null;
            }

            var evSuppressed = evReader.Value.Suppressed;

            try
            {
                // deserialize
                var serializationMap = GetSerializationMapForType(objectType);
                var instance = serializationMap.ConstructorFactory(Services);
                serializationMap.ReaderFactory(reader, serializer, instance, evReader.Value);
                return instance;
            }
            finally
            {
                // safety check: we are not leaking suppressed reader accidentally
                if (evSuppressed != evReader.Value.Suppressed)
                {
                    // Newtonsoft.Json may catch and consume the exception - kill the reader to be sure that deserialization can not continue
                    reader.Close();
                    throw new SecurityException("encrypted values state corrupted.");
                }
            }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var evSuppressLevel = evWriter.Value.SuppressedLevel;
            try
            {
                var serializationMap = GetSerializationMapForType(value.GetType());
                UsedSerializationMaps.Add(serializationMap);
                serializationMap.WriterFactory(writer, value, serializer, evWriter.Value, IsPostback);
            }
            finally
            {
                // safety check: we are not leaking suppressed reader accidentally
                if (evSuppressLevel != evWriter.Value.SuppressedLevel)
                {
                    // Newtonsoft.Json may catch and consume the exception - kill the writer to be sure that serialization can not continue
                    writer.Close();
                    throw new SecurityException("encrypted values state corrupted.");
                }
            }
        }

        /// <summary>
        /// Populates the specified JObject.
        /// </summary>
        public virtual void Populate(JsonReader reader, JsonSerializer serializer, object value)
        {
            var evSuppressed = evReader.Value.Suppressed;
            try
            {
                if (reader.TokenType == JsonToken.None) reader.Read();
                var serializationMap = GetSerializationMapForType(value.GetType());
                serializationMap.ReaderFactory(reader, serializer, value, evReader.Value);
            }
            finally
            {
                // safety check: we are not leaking suppressed reader accidentally
                if (evSuppressed != evReader.Value.Suppressed)
                {
                    // Newtonsoft.Json may catch and consume the exception - kill the reader to be sure that deserialization can not continue
                    reader.Close();
                    throw new SecurityException("encrypted values state corrupted.");
                }
            }
        }
    }
}
