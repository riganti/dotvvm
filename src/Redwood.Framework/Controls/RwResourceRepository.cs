
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// repository of resources accessible by name
    /// </summary>
    public class RwResourceRepository
    {
        /// <summary>
        /// Dictionary of resources
        /// </summary>
        public ConcurrentDictionary<string, RwResource> Resources { get; private set; }

        public RwResourceRepository Parent { get; set; }
        /// <summary>
        /// returns registered resource
        /// </summary>
        public RwResource Resolve(string name)
        {
            if (Resources.ContainsKey(name)) return Resources[name];
            else if (Parent != null) return Parent.Resolve(name);
            else throw new KeyNotFoundException("html resource was not found");
        }

        public bool IsRegistered(string name)
        {
            return Resources.ContainsKey(name) || (Parent != null && Parent.IsRegistered(name));
        }
        /// <summary>
        /// registers a new resource in collection
        /// </summary>
        public void Register(string name, RwResource resource, bool replaceIfExists = true)
        {
            if (replaceIfExists)
                Resources.AddOrUpdate(name, resource, (key, res) => resource);
            else if (!Resources.TryAdd(name, resource))
                throw new InvalidOperationException("name already registered");
        }

        /// <summary>
        /// Creates nested repository. All new registrations in the nested repo will not apply to this.
        /// </summary>
        public RwResourceRepository Nest()
        {
            return new RwResourceRepository(this);
        }

        public RwResourceRepository(RwResourceRepository parent)
        {
            this.Resources = new ConcurrentDictionary<string, RwResource>();
            this.Parent = parent;
        }

        public RwResourceRepository() : this(null) { }
    }
}
