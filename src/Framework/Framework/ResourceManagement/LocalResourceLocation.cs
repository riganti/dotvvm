using System;
using System.IO;
using System.Threading;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.ResourceManagement
{
    public abstract class LocalResourceLocation : ILocalResourceLocation
    {
        private Tuple<string, string>? cache;

        public string GetUrl(IDotvvmRequestContext context, string name)
        {
            if (this.cache is var (cachedName, cachedUrl))
            {
                // almost all resource locations have a single name
                if (cachedName == name)
                    return cachedUrl;
            }
            var url = context.Services.GetRequiredService<ILocalResourceUrlManager>().GetResourceUrl(this, context, name);

            var debug = context.Configuration.Debug;
            if (!debug && this.cache is null)
            {
                Interlocked.CompareExchange(ref this.cache, value: Tuple.Create(name, url), comparand: null);
            }
            return url;
        }

        public abstract Stream LoadResource(IDotvvmRequestContext context);
    }
}
