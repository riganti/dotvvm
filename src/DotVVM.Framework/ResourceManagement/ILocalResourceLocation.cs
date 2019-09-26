#nullable enable
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Represents resource located on the server, so it can be loaded to stream easily.
    /// </summary>
    public interface ILocalResourceLocation: IResourceLocation
    {
        Stream LoadResource(IDotvvmRequestContext context);
    }
}
