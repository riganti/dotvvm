
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// repository of named resources
    /// </summary>
    public class DotvvmResourceRepository : IDotvvmResourceRepository
    {
        /// <summary>
        /// Dictionary of resources
        /// </summary>
        [JsonIgnore]
        public ConcurrentDictionary<string, ResourceBase> Resources { get; private set; }

        [JsonIgnore]
        public ConcurrentDictionary<string, IDotvvmResourceRepository> Parents { get; set; }

        [JsonIgnore]
        public IList<IResourceProcessor> DefaultResourceProcessors { get; set; } = new List<IResourceProcessor>();

        /// <summary>
        /// Finds the resource with the specified name.
        /// </summary>
        public ResourceBase FindResource(string name)
        {
            if (Resources.ContainsKey(name)) return Resources[name];
            IDotvvmResourceRepository parent;
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

        public void RegisterNamedParent(string name, IDotvvmResourceRepository parent)
        {
            Parents[name] = parent;
        }

        /// <summary>
        /// Creates nested repository. All new registrations in the nested repo will not apply to this.
        /// </summary>
        public DotvvmResourceRepository Nest()
        {
            return new DotvvmResourceRepository(this);
        }

        public DotvvmResourceRepository(DotvvmResourceRepository parent) : this()
        {
            this.Resources = new ConcurrentDictionary<string, ResourceBase>();
            this.Parents.TryAdd("", parent);
        }

        public DotvvmResourceRepository()
        {
            this.Resources = new ConcurrentDictionary<string, ResourceBase>();
            this.Parents = new ConcurrentDictionary<string, IDotvvmResourceRepository>();
        }

        public NamedResource FindNamedResource(string name)
        {
            return new NamedResource(name, FindResource(name));
        }
    }
}
