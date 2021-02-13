using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

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
                try
                {
                    rt.Add(prop.Key, route["url"].Value<string>(), route["virtualPath"].Value<string>(), route["defaultValues"].ToObject<IDictionary<string, object>>());
                }
                catch (Exception error)
                {
                    rt.Add(prop.Key, new ErrorRoute(route["url"].Value<string>(), route["virtualPath"].Value<string>(), prop.Key, route["defaultValues"].ToObject<IDictionary<string, object>>(), error));
                }
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
                new JObject() {
                    ["url"] = route.Url,
                    ["virtualPath"] = route.VirtualPath,
                    ["defaultValues"] = JObject.FromObject(route.DefaultValues)
                }.WriteTo(writer);
            }
            writer.WriteEndObject();
        }

        sealed class ErrorRoute : RouteBase
        {
            private readonly Exception error;

            public ErrorRoute(string url, string virtualPath, string name, IDictionary<string, object> defaultValues, Exception error) : base(url, virtualPath, name, defaultValues)
            {
                this.error = error;
            }

            public override IEnumerable<string> ParameterNames => new string[0];

            public override string UrlWithoutTypes => base.Url;

            public override IDotvvmPresenter GetPresenter(IServiceProvider provider) => throw new InvalidOperationException($"Could not create route {RouteName}", error);

            public override bool IsMatch(string url, out IDictionary<string, object> values) => throw new InvalidOperationException($"Could not create route {RouteName}", error);

            protected override string BuildUrlCore(Dictionary<string, object> values) => throw new InvalidOperationException($"Could not create route {RouteName}", error);
            protected override void Freeze2()
            {
                // no mutable properties in this class
            }
        }
    }
}
