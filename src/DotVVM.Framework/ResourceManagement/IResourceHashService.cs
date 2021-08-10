#nullable enable
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Computes cryptographically secure hashes for given <see cref="ILocalResourceLocation"/>
    /// </summary>
    public interface IResourceHashService
    {
        string GetIntegrityHash(ILocalResourceLocation resource, IDotvvmRequestContext context);
        string GetVersionHash(ILocalResourceLocation resource, IDotvvmRequestContext context);
    }
}
