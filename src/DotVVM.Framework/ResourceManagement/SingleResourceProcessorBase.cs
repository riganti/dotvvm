using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public abstract class SingleResourceProcessorBase : IResourceProcessor
    {
        public IEnumerable<NamedResource> Process(IEnumerable<NamedResource> source)
        {
            var set = new HashSet<NamedResource>();
            foreach (var sourceResource in source)
            {

                if (Predicate(sourceResource.Name, sourceResource.Resource))
                {
                    foreach (var result in ProcessOne(sourceResource)) set.Add(result);
                }
                else set.Add(sourceResource);
            }
            return set;
        }

        public virtual bool Predicate(string name, IResource resource) => true;

        public abstract IEnumerable<NamedResource> ProcessOne(NamedResource resource);
    }
}
