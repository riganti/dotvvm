
using Newtonsoft.Json;
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
    public class RedwoodResourceRepository : IRedwoodResourceRepository
    {
        /// <summary>
        /// Dictionary of resources
        /// </summary>
        [JsonIgnore]
        public ConcurrentDictionary<string, ResourceBase> Resources { get; private set; }

        [JsonIgnore]
        public ConcurrentDictionary<string, IRedwoodResourceRepository> Parents { get; set; }

        /// <summary>
        /// Finds the resource with the specified name.
        /// </summary>
        public ResourceBase FindResource(string name)
        {
            if (Resources.ContainsKey(name)) return Resources[name];
            IRedwoodResourceRepository parent;
            if (name.Contains(':'))
            {
                var split = name.Split(new[] { ':' }, 2);
                if (Parents.TryGetValue(split[0], out parent))
                    return parent.FindResource(split[1]);
            }
            if (Parents.TryGetValue("", out parent))
            {
                var resource = parent.FindResource(name);
                if (resource != null) return resource;
            }
            return null;
        }

        /// <summary>
        /// registers a new resource in collection
        /// </summary>
        public void Register(string name, ResourceBase resource, bool replaceIfExists = true)
        {
            if (replaceIfExists)
                Resources.AddOrUpdate(name, resource, (key, res) => resource);
            else if (!Resources.TryAdd(name, resource))
                throw new InvalidOperationException("name already registered");
        }

        public void RegisterNamedParent(string name, IRedwoodResourceRepository parent)
        {
            Parents[name] = parent;
        }

        /// <summary>
        /// Creates nested repository. All new registrations in the nested repo will not apply to this.
        /// </summary>
        public RedwoodResourceRepository Nest()
        {
            return new RedwoodResourceRepository(this);
        }

        public RedwoodResourceRepository(RedwoodResourceRepository parent) : this()
        {
            this.Resources = new ConcurrentDictionary<string, ResourceBase>();
            this.Parents.TryAdd("", parent);
        }

        public RedwoodResourceRepository()
        {
            this.Resources = new ConcurrentDictionary<string, ResourceBase>();
            this.Parents = new ConcurrentDictionary<string, IRedwoodResourceRepository>();
        }

        internal void Register(object redwoodDebugResourceName)
        {
            throw new NotImplementedException();
        }
    }
}
