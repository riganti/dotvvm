﻿using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public class ReflectionAssemblyJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(Assembly).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string name)
            {
                return Assembly.Load(new AssemblyName(name));
            }
            else throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((Assembly)value).GetName().ToString());
        }
    }

    public class ReflectionTypeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(Type).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string name)
            {
                return Type.GetType(name) ?? throw new Exception($"Cannot find type {name}.");
            }
            else throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var t = ((Type)value);
            if (t.Assembly == typeof(string).Assembly)
                writer.WriteValue(t.FullName);
            else
                writer.WriteValue($"{t.FullName}, {t.Assembly.GetName().Name}");
        }
    }
    public class DotvvmTypeDescriptorJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(ITypeDescriptor).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string name)
            {
                return new ResolvedTypeDescriptor(Type.GetType(name) ?? throw new Exception($"Cannot find type {name}."));
            }
            else throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var t = ((ITypeDescriptor)value);
            var coreAssembly = typeof(string).Assembly.GetName().Name;
            var assembly = t.Assembly?.Split(new char[] { ',' }, 2)[0];
            if (assembly is null || assembly == coreAssembly)
                writer.WriteValue(t.FullName);
            else
                writer.WriteValue($"{t.FullName}, {assembly}");
        }
    }
}
