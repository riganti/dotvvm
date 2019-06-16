using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DefaultViewModelServerCache : IViewModelServerCache
    {
        private readonly SHA256 sha256;
        private readonly IViewModelServerStore viewModelStore;

        public DefaultViewModelServerCache(IViewModelServerStore viewModelStore)
        {
            sha256 = SHA256.Create();
            this.viewModelStore = viewModelStore;
        }

        public string StoreViewModel(IDotvvmRequestContext context, JObject viewModelToken)
        {
            var cacheData = viewModelToken.ToString(Newtonsoft.Json.Formatting.None);
            var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(cacheData)));
            
            viewModelStore.Store(hash, cacheData);
            return hash;
        }

        public JObject TryRestoreViewModel(IDotvvmRequestContext context, string viewModelCacheId, JObject viewModelDiffToken)
        {
            var cachedData = viewModelStore.Retrieve(viewModelCacheId);
            if (cachedData == null)
            {
                // the client needs to repeat the postback and send the full viewmode
                context.SetCachedViewModelMissingResponse();
                throw new DotvvmInterruptRequestExecutionException(InterruptReason.CachedViewModelMissing);
            }

            var result = JObject.Parse(cachedData);
            JsonUtils.Patch(result, viewModelDiffToken);
            return result;
        }
    }
}
