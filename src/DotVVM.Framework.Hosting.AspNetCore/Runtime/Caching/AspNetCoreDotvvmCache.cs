using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Runtime.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace DotVVM.Framework.Hosting.AspNetCore.Runtime.Caching
{
    public class AspNetCoreDotvvmCache : IDotvvmCache
    {
        private readonly IMemoryCache memoryCache;

        public AspNetCoreDotvvmCache(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public T Get<T>(object key) => GetOrAdd<T>(key, null);

        public object Remove(object key)
        {
            memoryCache.TryGetValue(key, out var value);
            memoryCache.Remove(key);
            return value;
        }

        public T GetOrAdd<T>(object key, Func<object, DotvvmCachedItem<T>> factoryFunc)
        {
            object value = null;
            if (!memoryCache.TryGetValue(key, out ICacheEntry cachedValue))
            {
                if (factoryFunc != null)
                {
                    var item = default(DotvvmCachedItem<T>);
                    item = factoryFunc.Invoke(key);
                    value = item.Value;
                    if (item.Value == null)
                        return default(T);

                    var entry = memoryCache.CreateEntry(key);

                    // priority
                    entry.Priority = item.Priority.ConvertToCacheItemPriority();

                    // Sliding
                    if (item.SlidingExpiration is TimeSpan span) entry.SetSlidingExpiration(span);

                    // absolute expiration
                    if (item.Expiration is TimeSpan expiration) entry.SetAbsoluteExpiration(expiration);
                    entry.SetValue(value);
                    memoryCache.Set(key, entry);
                }
            }
            else
            {
                if (cachedValue != null)
                {
                    value = cachedValue.Value;
                }
            }
            return value is T update ? update : default(T);
        }
    }
}
