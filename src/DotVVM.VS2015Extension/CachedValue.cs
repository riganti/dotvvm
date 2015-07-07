using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.VS2015Extension
{
    public class CachedValue<T> where T : class
    {

        private volatile T cache;
        private object locker = new object();

        public T GetOrRetrieve(Func<T> valueFactory)
        {
            if (cache == null)
            {
                lock (locker)
                {
                    if (cache == null)
                    {
                        cache = valueFactory();
                    }
                }
            }
            return cache;
        }

        public void ClearCachedValue()
        {
            lock (locker)
            {
                cache = null;
            }
        }

    }


    public class CachedValue<TKey, T> where T : class
    {

        private ConcurrentDictionary<TKey, T> cache = new ConcurrentDictionary<TKey, T>();

        public T GetOrRetrieve(TKey key, Func<T> valueFactory)
        {
            return cache.GetOrAdd(key, k => valueFactory());
        }

        public void ClearCachedValue(TKey key)
        {
            T result;
            cache.TryRemove(key, out result);
        }

        public void ClearCachedValues()
        {
            cache.Clear();
        }
    }
}
