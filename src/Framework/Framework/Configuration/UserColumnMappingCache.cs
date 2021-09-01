using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Configuration
{
    public class UserColumnMappingCache
    {
        private ConcurrentDictionary<Type, Dictionary<string, string>> mappingCache;
        private IViewModelSerializationMapper serializationMapper;

        public UserColumnMappingCache(IViewModelSerializationMapper serializationMapper)
        {
            this.serializationMapper = serializationMapper;
            this.mappingCache = new ConcurrentDictionary<Type, Dictionary<string, string>>();       
        }

        public IReadOnlyDictionary<string, string> GetMapping(Type type)
        {
            if (mappingCache.TryGetValue(type, out var columnsMapping))
                return columnsMapping;

            var serializationMap = serializationMapper.GetMap(type);

            columnsMapping = new Dictionary<string, string>();
            foreach (var property in serializationMap.Properties)
            {
                var resolvedName = property.Name;               
                if (resolvedName == property.PropertyInfo.Name)
                    continue;

                // Store only remapped columns
                columnsMapping.Add(property.PropertyInfo.Name, resolvedName);
            }
            mappingCache.TryAdd(type, columnsMapping);
            return columnsMapping;
        }
    }
}
