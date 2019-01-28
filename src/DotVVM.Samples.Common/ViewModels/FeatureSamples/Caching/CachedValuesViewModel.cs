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
        private readonly IDotvvmCache cache;

        public CachedValuesViewModel(IDotvvmCache cache)
        {
            this.cache = cache;
            Text = cache.Get<string>(key);
        }

        public void SetItem()
        {
            cache.GetOrAdd(key, k => new DotvvmCachedItem<string>("value"));
        }

        public void RemoveItem()
        {
            cache.Remove(key);
        }

        public string Text { get; set; }
    }
}
