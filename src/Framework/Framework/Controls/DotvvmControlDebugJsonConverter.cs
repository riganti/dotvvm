using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Controls
{
    internal class DotvvmControlDebugJsonConverter : JsonConverter<DotvvmBindableObject>
    {
        // public bool IncludeChildren { get; set; } = false;
        // public DotvvmConfiguration? Configuration { get; set; } = null;

        public override DotvvmBindableObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotImplementedException("Deserializing dotvvm control from JSON is not supported.");
        public override void Write(Utf8JsonWriter w, DotvvmBindableObject obj, JsonSerializerOptions options)
        {
            w.WriteStartObject();

            w.WritePropertyName("Control");
            w.WriteStringValue(obj.GetType().Name);
            
            w.WriteStartObject("Properties");
            foreach (var kvp in obj.Properties.OrderBy(p => (p.Key.DeclaringType.IsAssignableFrom(obj.GetType()), p.Key.Name)))
            {
                var p = kvp.Key;
                var rawValue = kvp.Value;
                var isAttached = !p.DeclaringType.IsAssignableFrom(obj.GetType());
                var name = isAttached ? p.DeclaringType.Name + "." + p.Name : p.Name;
                if (rawValue is null)
                    w.WriteNull(name);
                else if (rawValue is IBinding)
                    w.WriteString(name, rawValue.ToString());
                else
                {
                    w.WritePropertyName(name);
                    JsonSerializer.Serialize(w, rawValue, options);
                }
            }
            w.WriteEndObject();

            if (obj is DotvvmControl control)
            {
                w.WritePropertyName("LifecycleRequirements");
                w.WriteStringValue(control.LifecycleRequirements.ToString());
            }


            w.WriteEndObject();
        }
    }
}
