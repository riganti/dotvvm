using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace DotVVM.Framework.ResourceManagement
{
    public class ResourceRepositoryJsonConverter : JsonConverter<DotvvmResourceRepository>
    {
        public static Type? UnknownResourceType = null;
        static (string name, Type type)[] resourceTypeAliases = new [] {
            ("scripts", typeof(ScriptResource)),
            ("stylesheets", typeof(StylesheetResource)),
            ("null", typeof(NullResource))
        };

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DotvvmResourceRepository);
        }

        public override DotvvmResourceRepository? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
            // var jobj = JObject.Load(reader);
            // var repo = existingValue as DotvvmResourceRepository ?? new DotvvmResourceRepository();
            // foreach (var prop in jobj)
            // {
            //     if (resourceTypeAliases.FirstOrDefault(x => x.name == prop.Key) is var r && r.type != null)
            //     {
            //         DeserializeResources((JObject)prop.Value.NotNull(), r.type, serializer, repo);
            //     }
            //     else if (CompiledAssemblyCache.Instance!.FindType(prop.Key) is Type resourceType)
            //     {
            //         DeserializeResources((JObject)prop.Value.NotNull(), resourceType, serializer, repo);
            //     }
            //     else if (UnknownResourceType != null)
            //     {
            //         DeserializeResources((JObject)prop.Value.NotNull(), UnknownResourceType, serializer, repo);
            //     }
            //     else
            //         throw new NotSupportedException(string.Format("resource collection name {0} is not supported", prop.Key));
            // }
            // return repo;
        }

        // void DeserializeResources(JObject jobj, Type resourceType, JsonSerializer serializer, DotvvmResourceRepository repo)
        // {
        //     foreach (var resObj in jobj)
        //     {
        //         try
        //         {
        //             var resource = (IResource)serializer.Deserialize(resObj.Value!.CreateReader(), resourceType).NotNull();
        //             if (resource is LinkResourceBase linkResource)
        //             {
        //                 if (linkResource.Location == null)
        //                 {
        //                     linkResource.Location = new UnknownResourceLocation();
        //                 }
        //             }

        //             repo.Register(resObj.Key, resource);
        //         }
        //         catch (Exception ex)
        //         {
        //             repo.Register(resObj.Key, new DeserializationErrorResource(ex, resObj.Value));
        //         }
        //     }
        // }

        public override void Write(Utf8JsonWriter writer, DotvvmResourceRepository value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            var resources = value as DotvvmResourceRepository ?? throw new NotSupportedException();
            foreach (var (name, group) in (
                from k in resources.Resources
                orderby k.Key
                group k by k.Value.GetType() into g
                let niceName = resourceTypeAliases.FirstOrDefault(k => k.type == g.Key).name
                let name = niceName ?? g.Key.FullName
                orderby name
                select (name, g)))
            {
                writer.WritePropertyName(name);
                writer.WriteStartObject();
                foreach (var resource in group)
                {
                    writer.WritePropertyName(resource.Key);
                    JsonSerializer.Serialize(writer, resource.Value, resource.Value.GetType(), options);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }


        // public class DeserializationErrorResource : ResourceBase
        // {
        //     public Exception Error { get; }
        //     public JToken? Json { get; set; }
        //     public DeserializationErrorResource(Exception error, JToken? json) : base(ResourceRenderPosition.Head)
        //     {
        //         this.Error = error;
        //         this.Json = json;
        //     }

        //     public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
        //     {
        //         throw new NotSupportedException($"Resource could not be deserialized from '{(Json is null ? "null" : Json.ToString())}': \n{Error}");
        //     }
        // }
    }

    internal class UnknownResourceLocation : IResourceLocation
    {
        public string GetUrl(IDotvvmRequestContext context, string name)
        {
            throw new InvalidOperationException($"The Location of the resource {name} is unknown! This should happen only when deserializing DotVVM 1.0.x configuration.");
        }
    }
}
