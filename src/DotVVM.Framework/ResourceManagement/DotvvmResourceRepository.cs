
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
using DotVVM.Framework.Configuration;
using System.Collections;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Repository of named resources
    /// </summary>
    public sealed class DotvvmResourceRepository : IDotvvmResourceRepository
    {

        /// <summary>
        /// Dictionary of resources
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<string, IResource> Resources => ReadOnlyDictionaryWrapper<string, IResource>.WrapIfNeeded(_resources);

        // Resources and parent repositories can be added safely even when the page is already running
        // This is an exception from the rule that configuration is frozen after startup
        private readonly ConcurrentDictionary<string, IResource> _resources = new ConcurrentDictionary<string, IResource>();

        [JsonIgnore]
        public IReadOnlyDictionary<string, IDotvvmResourceRepository> Parents => ReadOnlyDictionaryWrapper<string, IDotvvmResourceRepository>.WrapIfNeeded(_parents);
        private readonly ConcurrentDictionary<string, IDotvvmResourceRepository> _parents = new ConcurrentDictionary<string, IDotvvmResourceRepository>();

        [JsonIgnore]
        public IList<IResourceProcessor> DefaultResourceProcessors => _defaultResourceProcessors;
        // The resource processors can not be changed after init
        private readonly FreezableList<IResourceProcessor> _defaultResourceProcessors = new FreezableList<IResourceProcessor>();

        /// <summary>
        /// Finds the resource with the specified name. Returns null if it's not found.
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
                _resources.AddOrUpdate(name, resource, (key, res) => { ThrowIfFrozen(); return resource; });
            }
            else if (!_resources.TryAdd(name, resource))
            {
                throw new InvalidOperationException($"A resource with the name '{name}' is already registered!");
            }
        }

        internal void RegisterViewModuleResources(ViewModuleImportResource importResource, ViewModuleInitResource initResource)
        {
            // view module resources can be registered after the application start - they are created when the view is actually compiled
            // this method is internal because it bypasses the frozen collection check
            // we don't throw if the resources exist - their names are hashed so if they are duplicate, we believe that they are actually the same
            _resources.TryAdd(importResource.ResourceName, importResource);
            _resources.TryAdd(initResource.ResourceName, initResource);
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
            // rewrite is allowed only if it's not frozen
            if (this.isFrozen)
                _parents[name] = parent;
            else
            {
                // when frozen, append is still allowed
                if (!_parents.TryAdd(name, parent))
                    ThrowIfFrozen();
            }
        }

        private void ValidateResourceName(string name)
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
            this._parents.TryAdd("", parent);
        }

        /// <summary> Finds the resource with the specified name. Throws an exception if the resource is not found. </summary>
        public NamedResource FindNamedResource(string name)
        {
            var r = FindResource(name);
            if (r is null)
                throw new Exception($"Could not find resource {name}");
            return new NamedResource(name, r);
        }

        private bool isFrozen = false;

        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error(nameof(DotvvmResourceRepository));
        }
        public void Freeze()
        {
            this.isFrozen = true;
            this._defaultResourceProcessors.Freeze();
        }
    }
}
