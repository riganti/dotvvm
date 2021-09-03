using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Runtime.Caching
{
    public interface IDotvvmCacheAdapter
    {
        T GetOrAdd<TKey, T>(TKey key, Func<TKey, DotvvmCachedItem<T>> factoryFunc)
            where TKey: notnull;

        T? Get<T>(object key);

        object? Remove(object key);
    }

    public class DotvvmCachedItem<T>
    {
        public DotvvmCachedItem(T value, DotvvmCacheItemPriority priority = DotvvmCacheItemPriority.Normal, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            Value = value;
            Priority = priority;
            Expiration = absoluteExpiration;
            SlidingExpiration = slidingExpiration;
        }

        public T Value { get; }
        public TimeSpan? Expiration { get; }
        public DotvvmCacheItemPriority Priority { get; }
        public TimeSpan? SlidingExpiration { get; }
    }

    public enum DotvvmCacheItemPriority
    {
        /// <summary>Cache items with this priority level are the most likely to be deleted from the cache as the server frees system memory.</summary>
        Low = 0,

        /// <summary>The default value for a cached item's priority is <see cref="F:DotVVM.Framework.Runtime.Caching.DotvvmCacheItemPriority.Normal" />.</summary>
        Normal = 1,

        /// <summary>Cache items with this priority level are the least likely to be deleted from the cache as the server frees system memory.</summary>
        High = 2,

        /// <summary>The cache items with this priority level will not be automatically deleted from the cache as the server frees system memory. However, items with this priority level are removed along with other items according to the item's absolute or sliding expiration time. </summary>
        NeverRemove = 3,
    }
}
