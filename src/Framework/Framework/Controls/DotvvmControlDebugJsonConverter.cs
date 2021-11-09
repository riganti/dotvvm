using System;
using System.Linq;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Controls
{
    internal class DotvvmControlDebugJsonConverter : JsonConverter
    {
        // public bool IncludeChildren { get; set; } = false;
        // public DotvvmConfiguration? Configuration { get; set; } = null;

        public override bool CanConvert(Type objectType) =>
            typeof(DotvvmBindableObject).IsAssignableFrom(objectType);
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
            throw new NotImplementedException("Deserializing dotvvm control from JSON is not supported.");
        public override void WriteJson(JsonWriter w, object valueObj, JsonSerializer serializer)
        {
            var obj = (DotvvmBindableObject)valueObj;
            w.WriteStartObject();

            w.WritePropertyName("Control");
            w.WriteValue(obj.GetType().Name);
            
            w.WritePropertyName("Properties");
            var properties = new JObject(
                from kvp in obj.Properties
                let p = kvp.Key
                let rawValue = kvp.Value
                let isAttached = !p.DeclaringType.IsAssignableFrom(obj.GetType())
                orderby !isAttached, p.Name
                let name = isAttached ? p.DeclaringType.Name + "." + p.Name : p.Name
                let value = rawValue is IBinding ? JValue.CreateString(rawValue.ToString()) :
                                                   JToken.FromObject(rawValue, serializer)
                select new JProperty(name, value)
            );
            properties.WriteTo(w);

            if (obj is DotvvmControl control)
            {
                w.WritePropertyName("LifecycleRequirements");
                w.WriteValue(control.LifecycleRequirements.ToString());
            }


            w.WriteEndObject();
        }
    }
}
