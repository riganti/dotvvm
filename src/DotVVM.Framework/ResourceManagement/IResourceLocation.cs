#nullable enable
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Represents a location where resource can be found.
    /// </summary>
    public interface IResourceLocation
    {
        string GetUrl(IDotvvmRequestContext context, string name);
    }
}
