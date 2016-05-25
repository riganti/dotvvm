using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Routing
{
    public class RouteTableJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(DotvvmRouteTable);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var rt = existingValue as DotvvmRouteTable;
            if (rt == null) return null;
            foreach (var prop in (JObject)JObject.ReadFrom(reader))
            {
                var route = (JObject)prop.Value;
                rt.Add(prop.Key, route["url"].Value<string>(), route["virtualPath"].Value<string>(), route["defaultValues"].ToObject<IDictionary<string, string>>());
            }
            return rt;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => WriteJson(writer, (DotvvmRouteTable)value, serializer);

        public void WriteJson(JsonWriter writer, DotvvmRouteTable value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach (var route in value)
            {
                writer.WritePropertyName(route.RouteName);
                new JObject()
                {
                    ["url"] = route.Url,
                    ["virtualPath"] = route.VirtualPath,
                    ["defaultValues"] = JObject.FromObject(route.DefaultValues)
                }.WriteTo(writer);
            }
            writer.WriteEndObject();
        }
    }
}
