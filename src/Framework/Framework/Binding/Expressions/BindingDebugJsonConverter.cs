using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Binding.Expressions
{
    internal class BindingDebugJsonConverter: JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            typeof(IBinding).IsAssignableFrom(objectType);
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
            throw new NotImplementedException("Deserializing dotvvm bindings from JSON is not supported.");
        public override void WriteJson(JsonWriter w, object valueObj, JsonSerializer serializer)
        {
            var obj = valueObj;
            w.WriteValue(obj.ToString());

            // w.WriteStartObject();
            // w.WritePropertyName("ToString");
            // w.WriteValue(obj.ToString());
            // var props = (obj as ICloneableBinding)?.GetAllComputedProperties() ?? Enumerable.Empty<IBinding>();
            // foreach (var p in props)
            // {
            //     var name = p.GetType().Name;
            //     w.WritePropertyName(name);
            //     serializer.Serialize(w, p);
            // }
            // w.WriteEndObject();
        }
    }
}
