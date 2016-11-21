using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    public class RemoteResourceLocation: IResourceLocation
    {
        public string Url { get; }
        public RemoteResourceLocation(string url)
        {
            this.Url = url;
        }

        public string GetUrl(IDotvvmRequestContext context, string name)
        {
            return context.TranslateVirtualPath(Url);
        }
    }
}
