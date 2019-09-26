
#nullable enable
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
using DotVVM.Framework.Utils;

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
        public ConcurrentDictionary<string, IResource> Resources { get; } = new ConcurrentDictionary<string, IResource>();

        [JsonIgnore]
        public ConcurrentDictionary<string, IDotvvmResourceRepository> Parents { get; } = new ConcurrentDictionary<string, IDotvvmResourceRepository>();

        [JsonIgnore]
        public IList<IResourceProcessor> DefaultResourceProcessors { get; } = new List<IResourceProcessor>();

        /// <summary>
        /// Finds the resource with the specified name. Returns null when it's not found.
        /// </summary>
        public IResource? FindResource(string name)
        {
            if (Resources.ContainsKey(name))
            {
                return Resources[name];
            }

            IDotvvmResourceRepository? parent;
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
                if (resource != null) return resource;
            }
            return null;
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
            ResourceUtils.AssertAcyclicDependencies(resource, name, FindResource);
            if (replaceIfExists)
            {
                Resources.AddOrUpdate(name, resource, (key, res) => resource);
            }
            else if (!Resources.TryAdd(name, resource))
            {
                throw new InvalidOperationException($"A resource with the name '{name}' is already registered!");
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
            if (!NamingUtils.IsValidResourceName(name))
            {
                throw new ArgumentException($"The resource name {name} is not valid! Only alphanumeric characters, dots, underscores and dashes are allowed! Also please note that two or more subsequent dots, underscores and dashes are reserved for internal use, and are allowed only in the middle of the resource name.");
            }
        }


        /// <summary>
        /// Creates nested repository. All new registrations in the nested repo will not apply to this.
        /// </summary>
        public DotvvmResourceRepository Nest()
        {
            return new DotvvmResourceRepository(this);
        }

        public DotvvmResourceRepository()
        {
        }

        public DotvvmResourceRepository(DotvvmResourceRepository parent)
        {
            this.Parents.TryAdd("", parent);
        }

        /// <summary> Finds the resource with the specified name. Throws an exception when the resource is not found. </summary>
        public NamedResource FindNamedResource(string name)
        {
            var r = FindResource(name);
            if (r is null)
                throw new Exception($"Could not find resource {name}");
            return new NamedResource(name, r);
        }
    }
}
