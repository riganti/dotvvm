using System;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Caching;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class InMemoryViewModelServerStore : IViewModelServerStore
    {
        private readonly IDotvvmCacheAdapter cacheAdapter;

        public TimeSpan CacheLifetime { get; set; } = TimeSpan.FromMinutes(5);

        public InMemoryViewModelServerStore(IDotvvmCacheAdapter cacheAdapter)
        {
            this.cacheAdapter = cacheAdapter;
        }

        public string Retrieve(string hash)
        {
            return cacheAdapter.Get<string>(BuildKey(hash));
        }

        public void Store(string hash, string cacheData)
        {
            cacheAdapter.GetOrAdd(BuildKey(hash), k => new DotvvmCachedItem<string>(cacheData, slidingExpiration: CacheLifetime));
        }

        private static string BuildKey(string hash)
        {
            return nameof(InMemoryViewModelServerStore) + "_" + hash;
        }

    }
}
