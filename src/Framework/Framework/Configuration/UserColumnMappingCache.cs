using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
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

            HotReloadMetadataUpdateHandler.UserColumnMappingCaches.Add(new(this));
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

        /// <summary> Clear cache when hot reload happens </summary>
        internal void ClearCaches(Type[] types)
        {
            foreach (var t in types)
                mappingCache.TryRemove(t, out _);
        }
    }
}
