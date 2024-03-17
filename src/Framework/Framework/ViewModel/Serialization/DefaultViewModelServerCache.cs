using System;
using System.Buffers;
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

        public ReadOnlyMemory<byte> TryRestoreViewModel(IDotvvmRequestContext context, string viewModelCacheId, JsonElement viewModelDiffToken)
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

            var unpacked = UnpackViewModel(cachedData);
            var unpackedBuffer = ArrayPool<byte>.Shared.Rent(unpacked.length + 2);
            try
            {
                var copiedLength = unpacked.data.CopyTo(unpackedBuffer, 1);
                if (copiedLength != unpacked.length)
                    throw new Exception($"DefaultViewModelServerCache.PackViewModel returned incorrect length");
                unpackedBuffer[0] = (byte)'{';
                unpackedBuffer[unpacked.length + 1] = (byte)'}';

                var resultJson = JsonNode.Parse(unpackedBuffer.AsSpan()[..(unpacked.length + 2)])!.AsObject();
                // TODO: this is just bad
                JsonUtils.Patch(resultJson, JsonObject.Create(viewModelDiffToken)!);
                var jsonData = new MemoryStream();
                using (var writer = new Utf8JsonWriter(jsonData))
                {
                    resultJson.WriteTo(writer);
                }
                return jsonData.ToMemory();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(unpackedBuffer);
            }
        }

        protected virtual byte[] PackViewModel(Stream data)
        {
            var output = new MemoryStream();
            using (var compressed = new System.IO.Compression.DeflateStream(output, System.IO.Compression.CompressionLevel.Fastest, leaveOpen: true))
            {
                data.CopyTo(compressed);
            }
            output.Write(BitConverter.GetBytes((int)data.Position), 0, 4); // 4 bytes uncompressed length at the end

            return output.ToArray();
        }

        protected virtual (Stream data, int length) UnpackViewModel(byte[] cachedData)
        {
            var inflate = new DeflateStream(new ReadOnlyMemoryStream(cachedData.AsMemory()[..^4]), CompressionMode.Decompress);
            var length = BitConverter.ToInt32(cachedData.AsSpan()[^4..]
#if !DotNetCore
                .ToArray(), 0
#endif
            );
            return (inflate, length);
        }
    }
}
