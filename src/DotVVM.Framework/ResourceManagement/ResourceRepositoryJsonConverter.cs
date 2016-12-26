using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    public class ResourceRepositoryJsonConverter : JsonConverter
    {
        public static Type UnknownResourceType = null;

        static Dictionary<string, Type> _resourceTypeNames;
        public static Dictionary<string, Type> ResourceTypeNames
        {
            get { return _resourceTypeNames ?? (_resourceTypeNames = GetResourceCollectionNames()); }
        }

        /// <summary>
        /// Finds all types derived from ResourceBase and creates a dictionary of their names, full names and names in <see cref="ResourceConfigurationCollectionNameAttribute"/>
        /// </summary>
        static Dictionary<string, Type> GetResourceCollectionNames()
        {
            var dict = new Dictionary<string, Type>();
            var dotvvmAssembly = typeof(DotvvmConfiguration).GetTypeInfo().Assembly.FullName;
            var resourceBaseType = typeof(ResourceBase);
            // for each type derived from ResourceBase
            foreach (var type in ReflectionUtils.GetAllAssemblies()
                .Where(a => a.GetReferencedAssemblies().Any(ra => ra.FullName == dotvvmAssembly) || a.FullName == dotvvmAssembly)
                .SelectMany(a => a.GetLoadableTypes().Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract && resourceBaseType.IsAssignableFrom(t))))
            {
                var attr = type.GetTypeInfo().GetCustomAttribute<ResourceConfigurationCollectionNameAttribute>();
                // name from attribute
                if (attr != null) dict.Add(attr.Name, type);
                // full name of type
                dict.Add(type.FullName, type);
                // simple name of type if not already occupied
                if (!dict.ContainsKey(type.Name)) dict.Add(type.Name, type);
            }
            return dict;
        }


        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DotvvmResourceRepository);
        }

        public IResource TryParseOldResourceFormat(JObject jobj, Type resourceType)
        {
            return null;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);
            var repo = existingValue as DotvvmResourceRepository ?? new DotvvmResourceRepository();
            foreach (var prop in jobj)
            {
                if (ResourceTypeNames.TryGetValue(prop.Key, out var type))
                {
                    DeserializeResources((JObject)prop.Value, type, serializer, repo);
                }
                else if(UnknownResourceType != null)
                {
                    DeserializeResources((JObject)prop.Value, UnknownResourceType, serializer, repo);
                }
                else
                    throw new NotSupportedException(string.Format("resource collection name {0} is not supported", prop.Key));
            }
            return repo;
        }

        void DeserializeResources(JObject jobj, Type resourceType, JsonSerializer serializer, DotvvmResourceRepository repo)
        {
            foreach (var resObj in jobj)
            {
                try
                {
                    var resource = serializer.Deserialize(resObj.Value.CreateReader(), resourceType) as IResource;
                    if (resource is LinkResourceBase linkResource)
                    {
                        if (linkResource.Location == null)
                        {
                            linkResource.Location = new UnknownResourceLocation();
                        }
                    }

                    repo.Register(resObj.Key, resource);
                }
                catch (Exception ex)
                {
                    repo.Register(resObj.Key, new DeserializationErrorResource(ex, resObj.Value));
                }
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            var resources = value as DotvvmResourceRepository ?? throw new NotSupportedException();
            foreach (var group in resources.Resources.GroupBy(k => k.Value.GetType())) {
                var name = ResourceTypeNames.First(k => k.Value == group.Key).Key;
                writer.WritePropertyName(name);
                writer.WriteStartObject();
                foreach (var resource in group) {
                    writer.WritePropertyName(resource.Key);
                    serializer.Serialize(writer, resource.Value);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

        public class DeserializationErrorResource: ResourceBase
        {
            public Exception Error { get; }
            public JToken Json { get; set; }
            public DeserializationErrorResource(Exception error, JToken json): base(ResourceRenderPosition.Head)
            {
                this.Error = error;
                this.Json = json;
            }

            public override void Render(IHtmlWriter writer, IDotvvmRequestContext context, string resourceName)
            {
                throw new NotSupportedException($"Resource could not be deserialized from '{Json.ToString()}': \n{Error}");
            }
        }
    }

    internal class UnknownResourceLocation : IResourceLocation
    {
        public string GetUrl(IDotvvmRequestContext context, string name)
        {
            throw new InvalidOperationException($"The Location of the resource {name} is unknown! This should happen only when deserializing DotVVM 1.0.x configuration.");
        }
    }
}
