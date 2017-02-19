using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Represents a resource located at remote server identified by a url.
    /// </summary>
    public class UrlResourceLocation: IResourceLocation
    {
        public string Url { get; }
        public UrlResourceLocation(string url)
        {
            this.Url = url;
        }

        public string GetUrl(IDotvvmRequestContext context, string name)
        {
            return context.TranslateVirtualPath(Url);
        }
    }

    /// <summary>
    /// Compatibility alias for UrlResourceLocation.
    /// Represents a resource located at remote server identified by a url.
    /// </summary> 
    [Obsolete("Use UrlResourceLocation instead.")]
    public class RemoteResourceLocation : UrlResourceLocation
    {
        public RemoteResourceLocation(string url) : base(url)
        {
        }
    }
}
