using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Runtime.Caching
{
    public interface IDotvvmCache
    {
        T GetOrAdd<T>(object key, Func<object, DotvvmCachedItem<T>> factoryFunc);

        T Get<T>(object key);

        object Remove(object key);
    }

    public class DotvvmCachedItem<T>
    {
        public DotvvmCachedItem(T value, DotvvmCacheItemPriority priority)
        {
            this.Value = value;
            Priority = priority;
        }

        public DotvvmCachedItem(T value)
        {
            this.Value = value;
        }

        public T Value;
        public TimeSpan? Expiration;
        public DotvvmCacheItemPriority Priority;
        public TimeSpan? SlidingExpiration;
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
