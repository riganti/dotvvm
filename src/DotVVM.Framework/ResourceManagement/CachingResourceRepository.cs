using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public abstract class CachingResourceRepository: IDotvvmResourceRepository
    {
        protected abstract IResource FindResource(string name);

        private ConcurrentDictionary<string, IResource> resourceCache = new ConcurrentDictionary<string, IResource>();
        IResource IDotvvmResourceRepository.FindResource(string name) =>
            resourceCache.GetOrAdd(name, FindResource);
    }
}
