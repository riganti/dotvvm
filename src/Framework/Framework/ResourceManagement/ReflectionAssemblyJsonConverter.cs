using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using FastExpressionCompiler;
using System;
using System.Collections.Generic;
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
            if (reader.TokenType == JsonTokenType.String)
            {
                return Assembly.Load(new AssemblyName(reader.GetString()!));
            }
            else throw new NotSupportedException();
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
            if (reader.TokenType == JsonTokenType.String)
            {
                var name = reader.GetString()!;
                return Type.GetType(name) ?? throw new Exception($"Cannot find type {name}.");
            }
            else throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, Type t, JsonSerializerOptions options)
        {
            if (t.Assembly == typeof(string).Assembly)
                writer.WriteStringValue(t.FullName);
            else
                writer.WriteStringValue($"{t.FullName}, {t.Assembly.GetName().Name}");
        }
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
            if (reader.TokenType == JsonTokenType.String)
            {
                var name = reader.GetString()!;
                ITypeDescriptor result = new ResolvedTypeDescriptor(Type.GetType(name) ?? throw new Exception($"Cannot find type {name}."));
                if (result is T t)
                    return t;
                else throw new NotSupportedException($"Cannot deserialize {typeToConvert}");
            }
            else throw new NotSupportedException();
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

    public class DataContextChangeAttributeConverter() : GenericWriterJsonConverter<DataContextChangeAttribute>(WriteObjectReflection)
    {
        internal static void WriteObjectReflection(Utf8JsonWriter writer, object attribute, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("$type", attribute.GetType().ToString());
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
    }

    public class DataContextManipulationAttributeConverter() : GenericWriterJsonConverter<DataContextStackManipulationAttribute>(DataContextChangeAttributeConverter.WriteObjectReflection)
    {
    }

    public class GenericWriterJsonConverter<T>(Action<Utf8JsonWriter, T, JsonSerializerOptions> write) : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => typeof(T).IsAssignableFrom(typeToConvert);
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            Activator.CreateInstance(typeof(Inner<>).MakeGenericType(typeof(T), typeToConvert), write) as JsonConverter;

        private class Inner<TActual>(Action<Utf8JsonWriter, T, JsonSerializerOptions> write) : JsonConverter<TActual>
        {
            public override TActual Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                throw new NotImplementedException();
            public override void Write(Utf8JsonWriter writer, TActual value, JsonSerializerOptions options) =>
                write(writer, (T)(object)value!, options);
        }
    }
}
