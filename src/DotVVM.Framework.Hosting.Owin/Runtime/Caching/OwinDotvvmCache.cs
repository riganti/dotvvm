using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using DotVVM.Framework.Runtime.Caching;

namespace DotVVM.Framework.Hosting.Owin.Runtime.Caching
{
    public class OwinDotvvmCache : IDotvvmCache
    {
        private Cache cache;
        private ConditionalWeakTable<object, string> keysTable;

        public OwinDotvvmCache()
        {
            cache = new Cache();
            keysTable = new ConditionalWeakTable<object, string>();
        }

        private void RemovedCallback(string key, object value, CacheItemRemovedReason r)
        {
            if (value is KeyValuePair<object, object> pair)
            {
                keysTable.Remove(pair.Key);
            }
        }

        public T GetOrAdd<T>(object key, Func<object, DotvvmCachedItem<T>> updateFunc)
        {
            var stringKey = GetOrAddStringKey(key);
            var value = cache.Get(stringKey);

            if (value == null)
            {
                var item = updateFunc?.Invoke(key);
                if (item == null) return default(T);

                var kvPair = new KeyValuePair<object, object>(key, item.Value);
                var aExpiration = item.Expiration != null ? DateTime.UtcNow.Add(item.Expiration.Value) : Cache.NoAbsoluteExpiration;
                var sExpiration = item.SlidingExpiration ?? Cache.NoSlidingExpiration;

                cache.Add(stringKey, kvPair, null, aExpiration, sExpiration, item.Priority.ConvertToCacheItemPriority(), RemovedCallback);
                value = kvPair;
            }
            var itemValue = value as KeyValuePair<object, object>?;
            return itemValue?.Value is T update ? update : default(T);
        }

        public T Get<T>(object key) => GetOrAdd<T>(key, null);

        private string GetOrAddStringKey(object key)
        {
            if (!keysTable.TryGetValue(key, out var value))
            {
                value = Guid.NewGuid().ToString();
                keysTable.Add(key, value);
            }
            return value;
        }

        public object Remove(object key)
        {
            if (keysTable.TryGetValue(key, out var stringKey))
            {
                if (cache.Remove(stringKey) is KeyValuePair<object, object> value)
                {
                    return value.Value;
                }
            }
            return null;
        }
    }
}
