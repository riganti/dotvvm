using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DefaultViewModelServerCache : IViewModelServerCache
    {
        private readonly IViewModelServerStore viewModelStore;

        public DefaultViewModelServerCache(IViewModelServerStore viewModelStore)
        {
            this.viewModelStore = viewModelStore;
        }

        public string StoreViewModel(IDotvvmRequestContext context, JObject viewModelToken)
        {
            var cacheData = PackViewModel(viewModelToken);
            var hash = Convert.ToBase64String(SHA256.Create().ComputeHash(cacheData));
            viewModelStore.Store(hash, cacheData);
            return hash;
        }

        public JObject TryRestoreViewModel(IDotvvmRequestContext context, string viewModelCacheId, JObject viewModelDiffToken)
        {
            var cachedData = viewModelStore.Retrieve(viewModelCacheId);
            if (cachedData == null)
            {
                throw new DotvvmInterruptRequestExecutionException(InterruptReason.CachedViewModelMissing);
            }

            var result = UnpackViewModel(cachedData);
            JsonUtils.Patch(result, viewModelDiffToken);
            return result;
        }

        protected virtual byte[] PackViewModel(JObject viewModelToken)
        {
            using (var ms = new MemoryStream())
            using (var bsonWriter = new BsonDataWriter(ms))
            {
                viewModelToken.WriteTo(bsonWriter);
                bsonWriter.Flush();

                return ms.ToArray();
            }
        }

        protected virtual JObject UnpackViewModel(byte[] cachedData)
        {
            using (var ms = new MemoryStream(cachedData))
            using (var bsonReader = new BsonDataReader(ms))
            {
                return (JObject)JToken.ReadFrom(bsonReader);                
            }
        }
    }
}
