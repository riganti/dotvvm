using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using DotVVM.Framework.Runtime.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace DotVVM.Framework.Hosting.AspNetCore.Runtime.Caching
{
    public class AspNetCoreDotvvmCacheAdapter : IDotvvmCacheAdapter
    {
        private readonly IMemoryCache memoryCache;

        public AspNetCoreDotvvmCacheAdapter(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public T Get<T>(object key) => GetOrAdd<object, T>(key, null);

        public object Remove(object key)
        {
            memoryCache.TryGetValue(key, out var value);
            memoryCache.Remove(key);
            return value;
        }

        public T GetOrAdd<Tkey, T>(Tkey key, Func<Tkey, DotvvmCachedItem<T>> factoryFunc)
        {
            if (memoryCache.TryGetValue(key, out ICacheEntry cachedValue) && cachedValue?.Value is T convertedValue)
            {
                return convertedValue;
            }

            if (factoryFunc == null)
                return default(T);

            var item = factoryFunc.Invoke(key);
            if (item == null || item.Value == null)
                return default(T);
            var value = item.Value;

            var entry = memoryCache.CreateEntry(key);

            entry.SetValue(value);
            entry.Priority = item.Priority.ConvertToCacheItemPriority();

            if (item.SlidingExpiration is TimeSpan span)
                entry.SetSlidingExpiration(span);

            if (item.Expiration is TimeSpan expiration)
                entry.SetAbsoluteExpiration(expiration);

            memoryCache.Set(key, entry);
            return value;
        }
    }
}
