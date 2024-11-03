using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Utils;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace DotVVM.Framework.Routing
{
    public class RouteTableJsonConverter : JsonConverter<DotvvmRouteTable>
    {
        public override DotvvmRouteTable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
            // var rt = new DotvvmRouteTable();
            // if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Expected StartObject");
            // reader.Read();

            // while (reader.TokenType == JsonTokenType.PropertyName)
            // {
            //     var routeName = reader.GetString();
            //     reader.Read();
            //     var route = (JObject)prop.Value.NotNull();
            //     try
            //     {
            //         rt.Add(prop.Key, route["url"].NotNull("route.url is required").Value<string>(), (route["virtualPath"]?.Value<string>()).NotNull("route.virtualPath is required"), route["defaultValues"]?.ToObject<IDictionary<string, object>>());
            //     }
            //     catch (Exception error)
            //     {
            //         rt.Add(prop.Key, new ErrorRoute(route["url"]?.Value<string>(), route["virtualPath"]?.Value<string>(), prop.Key, route["defaultValues"]?.ToObject<IDictionary<string, object?>>(), error));
            //     }
            // }
            // return rt;
        }

        public override void Write(Utf8JsonWriter writer, DotvvmRouteTable value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var route in value)
            {
                writer.WriteStartObject(route.RouteName);
                writer.WriteString("url", route.Url);
                writer.WriteString("virtualPath", route.VirtualPath);
                if (route.DefaultValues is not null)
                {
                    writer.WriteStartObject("defaultValues");
                    foreach (var (paramName, defaultValue) in route.DefaultValues)
                    {
                        writer.WritePropertyName(paramName);
                        JsonSerializer.Serialize(writer, defaultValue, options);
                    }
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

        sealed class ErrorRoute : RouteBase
        {
            private readonly Exception error;

            public ErrorRoute(string? url, string? virtualPath, string? name, IDictionary<string, object?>? defaultValues, Exception error)
                : base(url ?? "<unknown>", virtualPath ?? "<unknown>", name ?? "<unknown>", defaultValues)
            {
                this.error = error;
            }

            public override IEnumerable<string> ParameterNames { get; } = new string[0];

            public override IEnumerable<KeyValuePair<string, DotvvmRouteParameterMetadata>> ParameterMetadata { get; } = new KeyValuePair<string, DotvvmRouteParameterMetadata>[0];

            public override string UrlWithoutTypes => base.Url;

            public override IDotvvmPresenter GetPresenter(IServiceProvider provider) => throw new InvalidOperationException($"Could not create route {RouteName}", error);

            public override bool IsMatch(string url, [MaybeNullWhen(false)] out IDictionary<string, object?> values) => throw new InvalidOperationException($"Could not create route {RouteName}", error);

            protected internal override string BuildUrlCore(Dictionary<string, object?> values) => throw new InvalidOperationException($"Could not create route {RouteName}", error);

            protected override void Freeze2()
            {
                // no mutable properties in this class
            }
        }
    }
}
