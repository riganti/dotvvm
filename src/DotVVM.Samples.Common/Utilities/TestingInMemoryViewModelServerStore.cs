using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Samples.Common.Utilities
{
    /// <summary>
    /// Server-side viewmodel cache that never removes cached items. Do not use this in production environment - this class should be used only for the test purposes.
    /// </summary>
    public class TestingInMemoryViewModelServerStore : IViewModelServerStore
    {
        private readonly ConcurrentDictionary<string, byte[]> cache = new ConcurrentDictionary<string, byte[]>();

        public byte[] Retrieve(string hash)
        {
            return cache.TryGetValue(BuildKey(hash), out var data) ? data : null;
        }

        public void Store(string hash, byte[] cacheData)
        {
            cache.GetOrAdd(BuildKey(hash), cacheData);
        }

        private static string BuildKey(string hash)
        {
            return hash;
        }

    }
}
