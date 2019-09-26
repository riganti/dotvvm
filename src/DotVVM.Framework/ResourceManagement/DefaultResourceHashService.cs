#nullable enable
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public class DefaultResourceHashService : IResourceHashService
    {
        private ConditionalWeakTable<ILocalResourceLocation, byte[]> hashCache = new ConditionalWeakTable<ILocalResourceLocation, byte[]>();

        private Func<HashAlgorithm> hashFactory = SHA256.Create;
        private string hashFunctionName = "sha256";

        public void SetHashFunction(Func<HashAlgorithm> factory, string functionName)
        {
            this.hashFactory = factory;
            this.hashFunctionName = functionName;
            hashCache = new ConditionalWeakTable<ILocalResourceLocation, byte[]>();
        }

        protected virtual byte[] ComputeHash(Stream stream)
        {
            using (var hash = hashFactory())
            {
                return hash.ComputeHash(stream);
            }
        }

        protected byte[] GetHash(ILocalResourceLocation resourceLocation, IDotvvmRequestContext context)
        {
            if (context.Configuration.Debug)
            {
                using (var stream = resourceLocation.LoadResource(context))
                {
                    return ComputeHash(stream);
                }
            }
            else
            {
                return hashCache.GetValue(resourceLocation, l =>
                {
                    using (var stream = resourceLocation.LoadResource(context))
                    {
                        return ComputeHash(stream);
                    }
                });
            }
        }

        public string GetIntegrityHash(ILocalResourceLocation resource, IDotvvmRequestContext context) => 
            $"{hashFunctionName}-{Convert.ToBase64String(GetHash(resource, context))}";

        public string GetVersionHash(ILocalResourceLocation resource, IDotvvmRequestContext context) =>
            Convert.ToBase64String(GetHash(resource, context))
            .Remove(20) // take only 20 chars (120 bits)
            .Replace('/', '-').Replace('+', '_'); // `/` and `+` are not url-safe
    }
}
