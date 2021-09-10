using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Replaces specified resources by the registered bundles
    /// </summary>
    public class SpaModeResourceProcessor: IResourceProcessor
    {
        private readonly DotvvmConfiguration config;

        public SpaModeResourceProcessor(DotvvmConfiguration config)
        {
            this.config = config;
        }

        public IEnumerable<NamedResource> Process(IEnumerable<NamedResource> source)
        {
            foreach (var r in source)
            {
                if (r.Name == ResourceConstants.DotvvmResourceName + ".internal")
                    yield return this.config.Resources.FindNamedResource(ResourceConstants.DotvvmResourceName + ".internal-spa");
                else
                    yield return r;
            }
        }
    }
}
