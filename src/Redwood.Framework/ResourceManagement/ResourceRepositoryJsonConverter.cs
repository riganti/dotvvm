using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Redwood.Framework.ResourceManagement
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
            var redwoodAssembly = typeof(RedwoodConfiguration).Assembly.FullName;
            var resourceBaseType = typeof(ResourceBase);
            // for each type derived from ResourceBase
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetReferencedAssemblies().Any(ra => ra.FullName == redwoodAssembly) || a.FullName == redwoodAssembly)
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
            return objectType == typeof(RedwoodResourceRepository);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);
            var repo = existingValue as RedwoodResourceRepository;
            if (repo == null) repo = new RedwoodResourceRepository();
            foreach (var prop in jobj)
            {
                Type type;
                if (ResourceTypeNames.TryGetValue(prop.Key, out type))
                {
                    foreach (var resource in DeserializeResources(prop.Value, type, serializer))
                    {
                        repo.Register(resource);
                    }
                }
                else
                    throw new NotSupportedException(string.Format("resource collection name {0} is not supported", prop.Key));
            }
            return repo;
        }

        IEnumerable<ResourceBase> DeserializeResources(JToken jtoken, Type resourceType, JsonSerializer serializer)
        {
            if (jtoken.Type == JTokenType.Array)
                // read array of resources
                foreach (var resObj in jtoken as JArray)
                {
                    yield return serializer.Deserialize(resObj.CreateReader(), resourceType) as ResourceBase;
                }
            else if (jtoken.Type == JTokenType.Object)
                // read name-resource pairs
                foreach (var resObj in jtoken as JObject)
                {
                    var resource = serializer.Deserialize(resObj.Value.CreateReader(), resourceType) as ResourceBase;
                    resource.Name = resObj.Key;
                    yield return resource;
                }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("resource configuration serialization not supported");
        }
    }
}
