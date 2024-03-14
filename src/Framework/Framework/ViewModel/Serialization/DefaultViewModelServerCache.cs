using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class DefaultViewModelServerCache : IViewModelServerCache
    {
        private readonly IViewModelServerStore viewModelStore;

        public DefaultViewModelServerCache(IViewModelServerStore viewModelStore)
        {
            this.viewModelStore = viewModelStore;
        }

        public string StoreViewModel(IDotvvmRequestContext context, Stream data)
        {
            var cacheData = PackViewModel(data);
            var hash = Convert.ToBase64String(SHA256.Create().ComputeHash(cacheData));
            viewModelStore.Store(hash, cacheData);
            return hash;
        }

        public JsonElement TryRestoreViewModel(IDotvvmRequestContext context, string viewModelCacheId, JsonElement viewModelDiffToken)
        {
            var cachedData = viewModelStore.Retrieve(viewModelCacheId);
            var routeLabel = new KeyValuePair<string, object?>("route", context.Route!.RouteName);
            if (cachedData == null)
            {
                DotvvmMetrics.ViewModelCacheMiss.Add(1, routeLabel);
                throw new DotvvmInterruptRequestExecutionException(InterruptReason.CachedViewModelMissing);
            }

            DotvvmMetrics.ViewModelCacheHit.Add(1, routeLabel);
            DotvvmMetrics.ViewModelCacheBytesLoaded.Add(cachedData.Length, routeLabel);

            var result = UnpackViewModel(cachedData);
            var resultJson = JsonNode.Parse(result)!.AsObject();
            // TODO: this is just bad
            JsonUtils.Patch(resultJson, JsonObject.Create(viewModelDiffToken)!);
            var jsonData = new MemoryStream();
            using (var writer = new Utf8JsonWriter(jsonData))
            {
                resultJson.WriteTo(writer);
            }
            return JsonDocument.Parse(jsonData.ToMemory()).RootElement;
        }

        protected virtual byte[] PackViewModel(Stream data)
        {
            var output = new MemoryStream();
            using (var compressed = new System.IO.Compression.DeflateStream(output, System.IO.Compression.CompressionLevel.Fastest))
            {
                data.CopyTo(compressed);
            }
            return output.ToArray();
        }

        protected virtual Stream UnpackViewModel(byte[] cachedData)
        {
            return new DeflateStream(new MemoryStream(cachedData), CompressionMode.Decompress);
        }
    }
}
