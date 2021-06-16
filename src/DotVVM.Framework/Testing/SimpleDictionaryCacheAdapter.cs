#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Caching;

namespace DotVVM.Framework.Testing
{
    public class SimpleDictionaryCacheAdapter : IDotvvmCacheAdapter
    {
        ConcurrentDictionary<object, object?> cache = new ConcurrentDictionary<object, object?>();
        public T Get<T>(object key) => GetOrAdd<object, T>(key, null);

        public T GetOrAdd<TKey, T>(TKey key, Func<TKey, DotvvmCachedItem<T>>? factoryFunc)
            where TKey: notnull
        {
            if (factoryFunc == null)
                return (T)cache[key]!;
            else
                return (T)cache.GetOrAdd(key, _ => factoryFunc(key).Value)!;
        }

        public object? Remove(object key)
        {
            cache.TryRemove(key, out var obj);
            return obj;
        }
    }
}
