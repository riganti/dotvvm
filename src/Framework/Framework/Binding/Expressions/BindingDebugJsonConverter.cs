using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.Binding.Expressions
{
    internal class BindingDebugJsonConverter: JsonConverter<IBinding>
    {
        public override IBinding Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotImplementedException("Deserializing dotvvm bindings from JSON is not supported.");
        public override void Write(Utf8JsonWriter writer, IBinding obj, JsonSerializerOptions options)
        {
            writer.WriteStringValue(obj?.ToString());

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
