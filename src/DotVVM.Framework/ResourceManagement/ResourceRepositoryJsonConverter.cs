#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
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
        public static Type? UnknownResourceType = null;
        private static readonly object _resourceTypeNamesLock = new object();
        static ConcurrentDictionary<string, Type>? _resourceTypeNames;

        /// <summary>
        /// Finds all types derived from IResource and creates a dictionary of their names, full names and names in <see cref="ResourceConfigurationCollectionNameAttribute"/>
        /// </summary>
        public virtual ConcurrentDictionary<string, Type> GetResourceTypeNames()
        {
            if (_resourceTypeNames == null)
            {
                lock (_resourceTypeNamesLock)
                {
                    if (_resourceTypeNames == null)
                    {

                        _resourceTypeNames = new ConcurrentDictionary<string, Type>();
                        var dict = _resourceTypeNames;
                        var dotvvmAssembly = typeof(DotvvmConfiguration).GetTypeInfo().Assembly.FullName.NotNull();
                        var resourceBaseType = typeof(IResource);

                        var types = ResolveAllTypesDerivedFromIResource(dotvvmAssembly, resourceBaseType);
                        foreach (var type in types)
                        {
                            var attr = type.GetTypeInfo().GetCustomAttribute<ResourceConfigurationCollectionNameAttribute>();
                            // name from attribute
                            if (attr != null) dict.TryAdd(attr.Name, type);
                            // full name of type
                            dict.TryAdd(type.FullName.NotNull(), type);
                            // simple name of type if not already occupied
                            if (!dict.ContainsKey(type.Name)) dict.TryAdd(type.Name, type);
                        }
                    }
                }
            }

            return _resourceTypeNames;
        }

        protected virtual IEnumerable<Type> ResolveAllTypesDerivedFromIResource(string dotvvmAssembly, Type resourceBaseType)
        {
            // for each type derived from IResource
            var types = GetAllAssembliesLoadedAssemblies()
                .Where(a => a.GetReferencedAssemblies().Any(ra => ra.FullName == dotvvmAssembly) ||
                            a.FullName == dotvvmAssembly)
                .SelectMany(a => a.GetLoadableTypes().Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract && resourceBaseType.IsAssignableFrom(t)));
            return types;
        }

        protected virtual IEnumerable<Assembly> GetAllAssembliesLoadedAssemblies()
        {
            return ReflectionUtils.GetAllAssemblies();
        }


        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DotvvmResourceRepository);
        }

        public IResource? TryParseOldResourceFormat(JObject jobj, Type resourceType)
        {
            return null;
        }
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);
            var repo = existingValue as DotvvmResourceRepository ?? new DotvvmResourceRepository();
            foreach (var prop in jobj)
            {
                if (GetResourceTypeNames().TryGetValue(prop.Key, out var type))
                {
                    DeserializeResources((JObject)prop.Value, type, serializer, repo);
                }
                else if (UnknownResourceType != null)
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
                    var resource = (IResource)serializer.Deserialize(resObj.Value.CreateReader(), resourceType);
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
            foreach (var group in resources.Resources.GroupBy(k => k.Value.GetType()))
            {
                var name = GetResourceTypeNames().FirstOrDefault(k => k.Value == group.Key).Key;
                if (name == null) continue;

                writer.WritePropertyName(name);
                writer.WriteStartObject();
                foreach (var resource in group)
                {
                    writer.WritePropertyName(resource.Key);
                    serializer.Serialize(writer, resource.Value);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

        public class DeserializationErrorResource : ResourceBase
        {
            public Exception Error { get; }
            public JToken Json { get; set; }
            public DeserializationErrorResource(Exception error, JToken json) : base(ResourceRenderPosition.Head)
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
