using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using FastExpressionCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ResourceManagement
{
    public class ReflectionAssemblyJsonConverter : JsonConverter<Assembly>
    {
        public override Assembly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected an assembly name, but found {reader.TokenType}.");

            var assemblyName = reader.GetString();
            if (string.IsNullOrWhiteSpace(assemblyName))
                throw new JsonException("Assembly name cannot be empty.");

            try
            {
                return Assembly.Load(new AssemblyName(assemblyName));
            }
            catch (Exception ex) when (ex is FileNotFoundException or FileLoadException or BadImageFormatException)
            {
                throw new JsonException($"Assembly '{assemblyName}' could not be loaded.", ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, Assembly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(((Assembly?)value)?.GetName().ToString());
        }
    }

    public class ReflectionTypeJsonConverter : JsonConverter<Type>
    {
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected a type name, but found {reader.TokenType}.");

            var typeName = reader.GetString() ?? throw new JsonException("Type name cannot be empty.");
            if (string.IsNullOrWhiteSpace(typeName))
                throw new JsonException("Type name cannot be empty.");

            return ResolveType(typeName)
                ?? throw new JsonException($"Type '{typeName}' could not be resolved.");
        }

        public override void Write(Utf8JsonWriter writer, Type t, JsonSerializerOptions options)
        {
            if (t.Assembly == typeof(string).Assembly)
                writer.WriteStringValue(t.FullName);
            else
                writer.WriteStringValue($"{t.FullName}, {t.Assembly.GetName().Name}");
        }

        internal static Type? ResolveType(string typeName) =>
            Type.GetType(typeName, throwOnError: false)
            ?? AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(typeName, throwOnError: false))
                .FirstOrDefault(t => t is not null);
    }

    /// <summary> Formats type as C# type identifier </summary>
    public class DebugReflectionTypeJsonConverter(): GenericWriterJsonConverter<Type>(
        (writer, value, options) => {
            writer.WriteStringValue(value.ToCode());
        })
    {
    }

    public class DotvvmTypeDescriptorJsonConverter<T> : JsonConverter<T>
        where T: ITypeDescriptor
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default!;

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected a type descriptor name, but found {reader.TokenType}.");

            var typeName = reader.GetString() ?? throw new JsonException("Type descriptor name cannot be empty.");
            if (string.IsNullOrWhiteSpace(typeName))
                throw new JsonException("Type descriptor name cannot be empty.");

            var type = ReflectionTypeJsonConverter.ResolveType(typeName)
                ?? throw new JsonException($"Type '{typeName}' could not be resolved.");
            return (T)(ITypeDescriptor)new ResolvedTypeDescriptor(type);
        }

        public override void Write(Utf8JsonWriter writer, T t, JsonSerializerOptions options)
        {
            var coreAssembly = typeof(string).Assembly.GetName().Name;
            var assembly = t.Assembly?.Split(new char[] { ',' }, 2)[0];
            if (assembly is null || assembly == coreAssembly)
                writer.WriteStringValue(t.FullName);
            else
                writer.WriteStringValue($"{t.FullName}, {assembly}");
        }
    }

    public class DotvvmPropertyJsonConverter() : GenericWriterJsonConverter<IControlAttributeDescriptor>(
        (writer, value, options) => {
            writer.WriteStringValue(value.ToString());
        })
    {
    }

    public class DataContextChangeAttributeConverter() : GenericWriterJsonConverter<DataContextChangeAttribute>(WriteObjectReflection, ReadObjectReflection)
    {
        internal static void WriteObjectReflection(Utf8JsonWriter writer, object attribute, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("$type", attribute.GetType().AssemblyQualifiedName);
            var properties = attribute.GetType().GetProperties();
            foreach (var prop in properties)
            {
                if (prop.IsDefined(typeof(JsonIgnoreAttribute)) || prop.Name == "TypeId")
                    continue;

                writer.WritePropertyName(prop.Name);

                var value = prop.GetValue(attribute);

                // NB: RuntimeType is internal, so we need to use first public base type
                var valueType = value?.GetType() ?? typeof(object);
                while (!valueType.IsPublicType() || valueType == typeof(TypeInfo))
                {
                    valueType = valueType.BaseType!;
                }
                JsonSerializer.Serialize(writer, value, valueType, options);
            }
            writer.WriteEndObject();
        }

        internal static object ReadObjectReflection(JsonElement element, Type baseType, JsonSerializerOptions options)
        {
            if (!element.TryGetProperty("$type", out var typeNameElement) || typeNameElement.ValueKind != JsonValueKind.String)
                throw new JsonException("Data context attribute must specify its '$type'.");

            var typeName = typeNameElement.GetString()!;
            var type = ReflectionTypeJsonConverter.ResolveType(typeName)
                ?? throw new JsonException($"Type '{typeName}' could not be resolved.");
            if (!baseType.IsAssignableFrom(type))
                throw new JsonException($"Type '{typeName}' is not a {baseType.Name}.");

            var deserializationOptions = new JsonSerializerOptions(options);
            for (var i = deserializationOptions.Converters.Count - 1; i >= 0; i--)
            {
                if (deserializationOptions.Converters[i] is GenericWriterJsonConverter<DataContextChangeAttribute> or GenericWriterJsonConverter<DataContextStackManipulationAttribute>)
                    deserializationOptions.Converters.RemoveAt(i);
            }

            return JsonSerializer.Deserialize(element.GetRawText(), type, deserializationOptions)
                ?? throw new JsonException($"Data context attribute '{typeName}' could not be deserialized.");
        }
    }

    public class DataContextManipulationAttributeConverter() : GenericWriterJsonConverter<DataContextStackManipulationAttribute>(DataContextChangeAttributeConverter.WriteObjectReflection, DataContextChangeAttributeConverter.ReadObjectReflection)
    {
    }

    public class GenericWriterJsonConverter<T>(
        Action<Utf8JsonWriter, T, JsonSerializerOptions> write,
        Func<JsonElement, Type, JsonSerializerOptions, object>? read = null) : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeof(T).IsAssignableFrom(typeToConvert);
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            Activator.CreateInstance(typeof(Inner<>).MakeGenericType(typeof(T), typeToConvert), write, read) as JsonConverter;

        private class Inner<TActual>(
            Action<Utf8JsonWriter, T, JsonSerializerOptions> write,
            Func<JsonElement, Type, JsonSerializerOptions, object>? read) : JsonConverter<TActual>
        {
            public override TActual Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (read is null)
                    throw new NotSupportedException($"Deserializing {typeof(TActual)} is not supported.");

                using var document = JsonDocument.ParseValue(ref reader);
                return (TActual)read(document.RootElement, typeof(T), options);
            }

            public override void Write(Utf8JsonWriter writer, TActual value, JsonSerializerOptions options) =>
                write(writer, (T)(object)value!, options);
        }
    }
}
