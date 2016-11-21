using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public interface ILocalResourceLocation: IResourceLocation
    {
        Stream LoadResource(IDotvvmRequestContext context);
    }
}
