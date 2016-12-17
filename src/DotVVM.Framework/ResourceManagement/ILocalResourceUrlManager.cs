using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Gets url where the resource can be found and finds the resource location based on this url
    /// </summary>
    public interface ILocalResourceUrlManager
    {
        string GetResourceUrl(ILocalResourceLocation resource, IDotvvmRequestContext context, string name);
        ILocalResourceLocation FindResource(string url, IDotvvmRequestContext context, out string mimeType);
    }
}
