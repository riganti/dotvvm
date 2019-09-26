#nullable enable
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
}
