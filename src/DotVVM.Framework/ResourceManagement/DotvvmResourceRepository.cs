using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotVVM.Framework.Routing;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Repository of named resources
    /// </summary>
    public class DotvvmResourceRepository : IDotvvmResourceRepository
    {
        /// <summary>
        /// Dictionary of resources
        /// </summary>
        [JsonIgnore]
        public ConcurrentDictionary<string, IResource> Resources { get; }
            = new ConcurrentDictionary<string, IResource>();

        [JsonIgnore]
        public ConcurrentDictionary<string, IDotvvmResourceRepository> Parents { get; }
            = new ConcurrentDictionary<string, IDotvvmResourceRepository>();

        [JsonIgnore]
        public IList<IResourceProcessor> DefaultResourceProcessors { get; }
            = new List<IResourceProcessor>();

        public DotvvmResourceRepository()
        {
        }

        public DotvvmResourceRepository(DotvvmResourceRepository parent)
        {
            Parents.TryAdd("", parent);
        }

        /// <summary>
        /// Finds the resource with the specified name.
        /// </summary>
        public IResource FindResource(string name)
        {
            if (Resources.ContainsKey(name))
            {
                return Resources[name];
            }

            IDotvvmResourceRepository parent;
            if (name.Contains(':'))
            {
                var split = name.Split(new[] { ':' }, 2);
                if (Parents.TryGetValue(split[0], out parent))
                {
                    return parent.FindResource(split[1]);
                }
            }

            if (Parents.TryGetValue("", out parent))
            {
                var resource = parent.FindResource(name);
                if (resource != null)
                {
                    return resource;
                }
            }
            return null;
        }

        public NamedResource FindNamedResource(string name)
        {
            return new NamedResource(name, FindResource(name));
        }

        /// <summary>
        /// Creates nested repository. All new registrations in the nested repo will not apply to this.
        /// </summary>
        public DotvvmResourceRepository Nest()
        {
            return new DotvvmResourceRepository(this);
        }

        /// <summary>
        /// Registers a new resource in the repository.
        /// </summary>
        public void Register(string name, IResource resource, bool replaceIfExists = true)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            ValidateResourceName(name);
            ValidateResourceLocation(resource, name);
            if (replaceIfExists)
            {
                Resources.AddOrUpdate(name, resource, (key, res) => resource);
            }
            else if (!Resources.TryAdd(name, resource))
            {
                throw new InvalidOperationException($"A resource with the name '{name}' is already registered!");
            }
        }

        /// <summary>
        /// Registers a child resource repository.
        /// </summary>
        public void RegisterNamedParent(string name, IDotvvmResourceRepository parent)
        {
            ValidateResourceName(name);
            Parents[name] = parent;
        }

        protected virtual void ValidateResourceName(string name)
        {
            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9]+([._-][a-zA-Z0-9]+)*$"))
            {
                throw new ArgumentException($"The resource name {name} is not valid! Only alphanumeric characters, dots, underscores and dashes are allowed! Also please note that two or more subsequent dots, underscores and dashes are reserved for internal use, and are allowed only in the middle of the resource name.");
            }
        }

        private void ValidateResourceLocation(IResource resource, string name)
        {
            var linkResource = resource as LinkResourceBase;
            if (linkResource != null)
            {
                if (linkResource.Location == null)
                {
                    throw new DotvvmLinkResourceException($"The Location property of the resource '{name}' is not set.");
                }
            }
        }
    }
}
