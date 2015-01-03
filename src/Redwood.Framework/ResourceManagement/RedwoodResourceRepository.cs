
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.ResourceManagement
{
    /// <summary>
    /// repository of named resources
    /// </summary>
    public class RedwoodResourceRepository: IRedwoodResourceRepository
    {
        /// <summary>
        /// Dictionary of resources
        /// </summary>
        public ConcurrentDictionary<string, ResourceBase> Resources { get; private set; }

        public IRedwoodResourceRepository Parent { get; set; }

        /// <summary>
        /// Finds the resource with the specified name.
        /// </summary>
        public ResourceBase FindResource(string name)
        {
            if (Resources.ContainsKey(name)) return Resources[name];
            else if (Parent != null) return Parent.FindResource(name);
            else return null;
        }

        /// <summary>
        /// registers a new resource in collection
        /// </summary>
        public void Register(ResourceBase resource, bool replaceIfExists = true)
        {
            if (replaceIfExists)
                Resources.AddOrUpdate(resource.Name, resource, (key, res) => resource);
            else if (!Resources.TryAdd(resource.Name, resource))
                throw new InvalidOperationException("name already registered");
        }

        /// <summary>
        /// Creates nested repository. All new registrations in the nested repo will not apply to this.
        /// </summary>
        public RedwoodResourceRepository Nest()
        {
            return new RedwoodResourceRepository(this);
        }

        public RedwoodResourceRepository(RedwoodResourceRepository parent)
        {
            this.Resources = new ConcurrentDictionary<string, ResourceBase>();
            this.Parent = parent;
        }

        public RedwoodResourceRepository() : this(null) { }
    }
}
