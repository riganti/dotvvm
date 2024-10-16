using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;
using System.Security;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FastExpressionCompiler;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// A System.Text.Json converter that handles special features of DotVVM ViewModel serialization.
    /// </summary>
    public class ViewModelJsonConverter : JsonConverterFactory
    {
        private readonly IViewModelSerializationMapper viewModelSerializationMapper;

        public ViewModelJsonConverter(IViewModelSerializationMapper viewModelSerializationMapper)
        {
            this.viewModelSerializationMapper = viewModelSerializationMapper;
        }
        public static bool CanConvertType(Type type) =>
            !ReflectionUtils.IsEnumerable(type) &&
            ReflectionUtils.IsComplexType(type) &&
            !ReflectionUtils.IsJsonDom(type) &&
            !type.IsDefined(typeof(JsonConverterAttribute), true) &&
            type != typeof(object);

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return CanConvertType(objectType);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) => (JsonConverter)GetDotvvmConverter(typeToConvert);
        private JsonConverter CreateConverterReally(Type typeToConvert) =>
            (JsonConverter)Activator.CreateInstance(typeof(VMConverter<>).MakeGenericType(typeToConvert), this)!;

        public VMConverter<T> CreateConverter<T>() => new VMConverter<T>(this);

        private ConcurrentDictionary<Type, IDotvvmJsonConverter> converterCache = new();
        internal IDotvvmJsonConverter GetDotvvmConverter(Type type) =>
            converterCache.GetOrAdd(type, t => (IDotvvmJsonConverter)CreateConverterReally(t));
        internal JsonConverter GetConverter(Type type) =>
            (JsonConverter)GetDotvvmConverter(type);

        public class VMConverter<T>(ViewModelJsonConverter factory): JsonConverter<T>, IDotvvmJsonConverter<T>
        {
            ViewModelSerializationMap<T> SerializationMap { get; } = factory.viewModelSerializationMapper.GetMap<T>();

            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                this.Read(ref reader, typeToConvert, options, DotvvmSerializationState.Current!);
            public T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, DotvvmSerializationState state)
            {
                if (state is null)
                    throw new ArgumentNullException(nameof(state), "DotvvmSerializationState must be created before calling the ViewModelJsonConverter.");
                if (typeof(T) != typeToConvert)
                    throw new ArgumentException("typeToConvert must be the same as T", nameof(typeToConvert));

                if (reader.TokenType == JsonTokenType.Null)
                {
                    Debug.Assert(!typeof(T).IsValueType);
                    return default!;
                }

                ReadObjectStart(ref reader);
                var evSuppressed = state.EVReader!.Suppressed;

                try
                {
                    // deserialize
                    var result = SerializationMap.ReaderFactory.Invoke(ref reader, options, default!, false, state.EVReader, state);
                    ReadEndObject(ref reader);
                    return result;
                }
                finally
                {
                    // safety check: we are not leaking suppressed reader accidentally
                    if (evSuppressed != state.EVReader.Suppressed)
                    {
                        // read everything to prevent any further deserialization
                        while (reader.Read())
                            ;
                        throw new SecurityException("encrypted values state corrupted.");
                    }
                }
            }

            static void ReadObjectStart(ref Utf8JsonReader reader)
            {
                if (reader.TokenType == JsonTokenType.None) reader.Read();
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException($"Cannot deserialize '{typeof(T).ToCode()}': Expected StartObject token, but reader.TokenType = {reader.TokenType}");
                reader.Read();
            }
            static void ReadEndObject(ref Utf8JsonReader reader)
            {
                if (reader.TokenType != JsonTokenType.EndObject)
                    throw new JsonException($"Expected EndObject token, but reader.TokenType = {reader.TokenType}");
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
                this.Write(writer, value, options, DotvvmSerializationState.Current!);
            public void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options, DotvvmSerializationState state, bool requireTypeField = true, bool wrapObject = true)
            {
                if (state is null)
                    throw new ArgumentNullException(nameof(state), "DotvvmSerializationState must be created before calling the ViewModelJsonConverter.");
                if (value is null)
                {
                    writer.WriteNullValue();
                    return;
                }
                var evSuppressLevel = state.EVWriter!.SuppressedLevel;
                try
                {
                    if (requireTypeField)
                    {
                        // $type not required -> serialization map is already known from the parent
                        state.UsedSerializationMaps.Add(SerializationMap);
                    }
                    if (wrapObject)
                    {
                        writer.WriteStartObject();
                    }
                    state.EVWriter.Nest();

                    SerializationMap.WriterFactory.Invoke(writer, value, options, requireTypeField, state.EVWriter, state);

                    if (wrapObject)
                    {
                        writer.WriteEndObject();
                    }
                    state.EVWriter.End();
                }
                finally
                {
                    // safety check: we are not leaking suppressed reader accidentally
                    if (evSuppressLevel != state.EVWriter.SuppressedLevel)
                    {
                        writer.Dispose(); // make sure nothing else is written
                        throw new SecurityException("encrypted values state corrupted.");
                    }
                }
            }

            /// <summary>
            /// Populates the specified JObject.
            /// </summary>
            public T? Populate(ref Utf8JsonReader reader, JsonSerializerOptions options, T value) =>
                this.Populate(ref reader, typeof(T), value, options, DotvvmSerializationState.Current!);
            public T Populate(ref Utf8JsonReader reader, Type typeToConvert, T value, JsonSerializerOptions options, DotvvmSerializationState state)
            {
                if (state is null)
                    throw new ArgumentNullException(nameof(state), "DotvvmSerializationState must be created before calling the ViewModelJsonConverter.");

                if (reader.TokenType == JsonTokenType.Null)
                {
                    Debug.Assert(!typeof(T).IsValueType);
                    return default!;
                }
                ReadObjectStart(ref reader);
                var evSuppressed = state.EVReader!.Suppressed;
                try
                {
                    var result = SerializationMap.ReaderFactory.Invoke(ref reader, options, value, value is not null, state.EVReader, state);
                    ReadEndObject(ref reader);
                    return result;
                }
                finally
                {
                    // safety check: we are not leaking suppressed reader accidentally
                    if (evSuppressed != state.EVReader.Suppressed)
                    {
                        // read everything to prevent any further deserialization
                        while (reader.Read())
                            ;
                        throw new SecurityException("encrypted values state corrupted.");
                    }
                }
            }

            public object? ReadUntyped(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, DotvvmSerializationState state) =>
                this.Read(ref reader, typeToConvert, options, state);
            public object? PopulateUntyped(ref Utf8JsonReader reader, Type typeToConvert, object? value, JsonSerializerOptions options, DotvvmSerializationState state) =>
                this.Populate(ref reader, typeof(T), (T)value!, options, state);
            public void WriteUntyped(Utf8JsonWriter writer, object? value, JsonSerializerOptions options, DotvvmSerializationState state, bool requireTypeField = true, bool wrapObject = true) =>
                this.Write(writer, (T)value!, options, state, requireTypeField, wrapObject);
        }
    }

    public class DotvvmSerializationState: IDisposable
    {
        [ThreadStatic]
        private static DotvvmSerializationState? current;

        internal static DotvvmSerializationState? Current => current;

        internal static DotvvmSerializationState Create(
            bool isPostback,
            IServiceProvider services,
            JsonObject? readEncryptedValues = null)
        {
            if (current is not null)
                throw new InvalidOperationException("ThreadStatic DotvvmSerializationState is already set.");
            return current = new DotvvmSerializationState(isPostback, services, readEncryptedValues);
        }


        public IServiceProvider Services { get; }
        public bool IsPostback { get; }
        public JsonObject? ReadEncryptedValues { get; }
        public EncryptedValuesReader? EVReader { get; }
        public EncryptedValuesWriter? EVWriter { get; }
        private MemoryStream? writeEncryptedValuesData;
        private Utf8JsonWriter? writeEncryptedValuesWriter;
        public HashSet<ViewModelSerializationMap> UsedSerializationMaps { get; } = new();

        public MemoryStream? WriteEncryptedValues
        {
            get
            {
                this.writeEncryptedValuesWriter?.Flush();
                return writeEncryptedValuesData;
            }
        }


        private DotvvmSerializationState(bool isPostback, IServiceProvider services, JsonObject? readEncryptedValues)
        {
            Services = services;
            IsPostback = isPostback;
            ReadEncryptedValues = readEncryptedValues;
            if (readEncryptedValues is not null)
            {
                EVReader = new EncryptedValuesReader(readEncryptedValues);
            }
            else
            {
                writeEncryptedValuesData = new MemoryStream();
                writeEncryptedValuesWriter = new Utf8JsonWriter(writeEncryptedValuesData, new JsonWriterOptions { Indented = false, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                EVWriter = new EncryptedValuesWriter(writeEncryptedValuesWriter);
            }

        }

        public void Dispose()
        {
            if (current != this)
                throw new InvalidOperationException("ThreadStatic DotvvmSerializationState is different.");
            current = null;
        }
    }
}
