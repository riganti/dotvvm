using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.ResourceManagement
{
    public class ResourceRepositoryJsonConverter : JsonConverter
    {
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
            var dotvvmAssembly = typeof(DotvvmConfiguration).Assembly.FullName;
            var resourceBaseType = typeof(ResourceBase);
            // for each type derived from ResourceBase
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetReferencedAssemblies().Any(ra => ra.FullName == dotvvmAssembly) || a.FullName == dotvvmAssembly)
                .SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && resourceBaseType.IsAssignableFrom(t))))
            {
                var attr = type.GetCustomAttribute<ResourceConfigurationCollectionNameAttribute>();
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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);
            var repo = existingValue as DotvvmResourceRepository;
            if (repo == null) repo = new DotvvmResourceRepository();
            foreach (var prop in jobj)
            {
                Type type;
                if (ResourceTypeNames.TryGetValue(prop.Key, out type))
                {
                    DeserializeResources((JObject)prop.Value, type, serializer, repo);
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
                var resource = serializer.Deserialize(resObj.Value.CreateReader(), resourceType) as ResourceBase;
                repo.Register(resObj.Key, resource);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("resource configuration serialization not supported");
        }
    }
}
