using System;

namespace DotVVM.Framework.Runtime.Caching
{
    public class DefaultDotvvmCacheAdapter : IDotvvmCacheAdapter
    {
        readonly SimpleLruDictionary<object, object> lru;

        public DefaultDotvvmCacheAdapter(int generationSize = 1000, int generationTickTimeSec = 5*60)
        {
            lru = new SimpleLruDictionary<object, object>(generationSize, TimeSpan.FromSeconds(generationTickTimeSec));
        }

        public T Get<T>(object key) => lru.TryGetValue(key, out var result) ? (T)result : default!;
        public T GetOrAdd<Tkey, T>(Tkey key, Func<Tkey, DotvvmCachedItem<T>> factoryFunc) where Tkey : notnull =>
            (T)lru.GetOrCreate(key, key => factoryFunc((Tkey)key).Value);
        public object Remove(object key) => lru.Remove(key, out var oldValue) ? oldValue : null;
    }
}
