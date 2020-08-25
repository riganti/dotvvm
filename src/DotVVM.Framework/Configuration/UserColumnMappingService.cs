#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Configuration
{
    public class UserColumnMappingCache
    {
        private ConcurrentDictionary<Type, Dictionary<string, string>> mappingCache;
        private IPropertySerialization propertySerialization;

        public UserColumnMappingCache(IPropertySerialization propertySerialization)
        {
            this.propertySerialization = propertySerialization;
            this.mappingCache = new ConcurrentDictionary<Type, Dictionary<string, string>>();       
        }

        public Dictionary<string, string> GetMapping(Type type)
        {
            if (mappingCache.TryGetValue(type, out var mapping))
                return mapping;

            mapping = new Dictionary<string, string>();
            foreach (var property in type.GetProperties())
            {
                var resolvedName = propertySerialization.ResolveName(property);
                if (resolvedName == property.Name)
                    continue;

                mapping.Add(property.Name, resolvedName);
            }
            mappingCache.TryAdd(type, mapping);
            return mapping;
        }
    }
}
