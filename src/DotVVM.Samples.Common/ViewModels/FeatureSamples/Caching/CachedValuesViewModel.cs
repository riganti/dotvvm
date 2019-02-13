using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Caching;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Caching
{
    public class CachedValuesViewModel : DotvvmViewModelBase
    {
        private static object key = new object();
        private readonly IDotvvmCacheAdapter cacheAdapter;

        public CachedValuesViewModel(IDotvvmCacheAdapter cacheAdapter)
        {
            this.cacheAdapter = cacheAdapter;
            Text = cacheAdapter.Get<string>(key);
        }

        public void SetItem()
        {
            cacheAdapter.GetOrAdd(key, k => new DotvvmCachedItem<string>("value"));
        }

        public void RemoveItem()
        {
            cacheAdapter.Remove(key);
        }

        public string Text { get; set; }
    }
}
