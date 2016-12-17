using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Replaces specified resources by the registered bundles
    /// </summary>
    public class BundlingResourceProcessor : SingleResourceProcessorBase
    {
        public Dictionary<string, NamedResource> BundleInverseIndex { get; } = new Dictionary<string, NamedResource>();

        public override IEnumerable<NamedResource> ProcessOne(NamedResource resource)
            => new[] { BundleInverseIndex[resource.Name] };

        public override bool Predicate(string name, IResource resource)
            => BundleInverseIndex.ContainsKey(name);

        public void RegisterBundle(NamedResource bundleResource, params string[] resolves)
        {
            foreach (var res in resolves)
            {
                BundleInverseIndex.Add(res, bundleResource);
            }
        }
    }
}
