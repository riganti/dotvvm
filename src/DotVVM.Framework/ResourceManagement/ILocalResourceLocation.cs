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

    /// <summary>
    /// Can get physical location of the file for debugging purposes. In that directory can be located asociated source maps and based on file will be the resource refreshed.
    /// </summary>
    public interface IDebugFileLocalLocation: ILocalResourceLocation
    {
        string GetFilePath(IDotvvmRequestContext context);
    }
}
