using System;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Caching;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class InMemoryViewModelServerStore : IViewModelServerStore
    {
        private readonly IDotvvmCacheAdapter cacheAdapter;
        private readonly IOptions<ViewModelServerCacheConfiguration> cacheConfigurationOptions;

        public InMemoryViewModelServerStore(IDotvvmCacheAdapter cacheAdapter, IOptions<ViewModelServerCacheConfiguration> cacheConfigurationOptions)
        {
            this.cacheAdapter = cacheAdapter;
            this.cacheConfigurationOptions = cacheConfigurationOptions;
        }

        public byte[] Retrieve(string hash)
        {
            return cacheAdapter.Get<byte[]>(BuildKey(hash));
        }

        public void Store(string hash, byte[] cacheData)
        {
            cacheAdapter.GetOrAdd(BuildKey(hash), k => new DotvvmCachedItem<byte[]>(cacheData, DotvvmCacheItemPriority.Low, slidingExpiration: cacheConfigurationOptions.Value.CacheLifetime));
        }

        private static string BuildKey(string hash)
        {
            return nameof(InMemoryViewModelServerStore) + "_" + hash;
        }

    }
}
