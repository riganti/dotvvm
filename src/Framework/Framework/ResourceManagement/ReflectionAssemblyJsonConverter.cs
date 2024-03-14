using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

    public class DotvvmPropertyJsonConverter : JsonConverter<IControlAttributeDescriptor>
    {
        public override IControlAttributeDescriptor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, IControlAttributeDescriptor value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public class DataContextChangeAttributeConverter : JsonConverter<DataContextChangeAttribute>
    {
        public override DataContextChangeAttribute? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, DataContextChangeAttribute attribute, JsonSerializerOptions options)
        {
            WriteObjectReflction(writer, attribute, options);
        }

        internal static void WriteObjectReflction(Utf8JsonWriter writer, object attribute, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("$type", attribute.GetType().ToString());
            var properties = attribute.GetType().GetProperties();
            foreach (var prop in properties)
            {
                if (prop.IsDefined(typeof(JsonIgnoreAttribute)) || prop.Name == "TypeId")
                    continue;

                writer.WritePropertyName(prop.Name);

                JsonSerializer.Serialize(writer, prop.GetValue(attribute), options);
            }
            writer.WriteEndObject();
        }
    }

    public class DataContextManipulationAttributeConverter : JsonConverter<DataContextStackManipulationAttribute>
    {
        public override DataContextStackManipulationAttribute Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, DataContextStackManipulationAttribute value, JsonSerializerOptions options)
        {
            DataContextChangeAttributeConverter.WriteObjectReflction(writer, value, options);
        }
    }
}
