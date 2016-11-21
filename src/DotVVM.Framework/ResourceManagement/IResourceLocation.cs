using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public interface IResourceLocation
    {
        string GetUrl(IDotvvmRequestContext context, string name);
    }
}
