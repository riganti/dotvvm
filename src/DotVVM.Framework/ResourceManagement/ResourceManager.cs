using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Parser;
using System.Threading;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Holds all required resources and render them to the page in correct order.
    /// </summary>
    public class ResourceManager
    {
        private readonly DotvvmConfiguration configuration;
        private List<string> requiredResourcesOrdered = new List<string>();
        private Dictionary<string, ResourceBase> requiredResources = new Dictionary<string, ResourceBase>();
        private List<IResourceProcessor> processors = new List<IResourceProcessor>();
        private int nonameCtr = 0;

        public IReadOnlyCollection<string> RequiredResources
        {
            get { return requiredResourcesOrdered.AsReadOnly(); }
        }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceManager"/> class.
        /// </summary>
        public ResourceManager(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Adds the required resource with specified name.
        /// </summary>
        public void AddRequiredResource(string name)
        {
            var resource = configuration.Resources.FindResource(name);
            if (resource == null)
            {
                ThrowResourceNotFound(name);
            }

            AddRequiredResourceCore(name, resource);
        }

        private void AddRequiredResourceCore(ResourceBase resource) => AddRequiredResourceCore("__noname_" + nonameCtr++, resource);

        /// <summary>
        /// Adds the resource and checks name conflicts.
        /// </summary>
        private void AddRequiredResourceCore(string name, ResourceBase resource)
        {
            ResourceBase originalResource;
            if (requiredResources.TryGetValue(name, out originalResource))
            {
                if (originalResource.Url != resource.Url)
                {
                    ThrowNonUniqueName(name);
                }
            }
            else
            {
                foreach (var dep in resource.Dependencies)
                {
                    AddRequiredResource(dep);
                }
                requiredResourcesOrdered.Add(name);
                requiredResources[name] = resource;
            }
        }

        /// <summary>
        /// Adds the required script file.
        /// </summary>
        public void AddRequiredScriptFile(string name, string url, params string[] dependentResourceNames)
        {
            AddRequiredResourceCore(name, new ScriptResource() { Url = url, Dependencies = dependentResourceNames });
        }

        /// <summary>
        /// Adds the required stylesheet file.
        /// </summary>
        public void AddRequiredStylesheetFile(string name, string url, params string[] dependentResourceNames)
        {
            AddRequiredResourceCore(name, new StylesheetResource() { Url = url, Dependencies = dependentResourceNames });
        }

        /// <summary>
        /// Adds the specified piece of javascript that will be executed when the page is loaded.
        /// </summary>
        public void AddStartupScript(string name, string javascriptCode, params string[] dependentResourceNames)
        {
            AddRequiredResourceCore(name, new InlineScriptResource() { Code = javascriptCode, Dependencies = dependentResourceNames });
        }

        /// <summary>
        /// Adds the specified piece of javascript that will be executed when the page is loaded.
        /// </summary>
        public void AddStartupScript(string javascriptCode, params string[] dependentResourceNames)
        {
            AddRequiredResourceCore(new InlineScriptResource() { Code = javascriptCode, Dependencies = dependentResourceNames });
        }

        /// <summary>
        /// Adds the globalization file for current thread culture.
        /// </summary>
        public void AddCurrentCultureGlobalizationResource()
        {
            AddRequiredResource(string.Format(Constants.GlobalizeCultureResourceName, Thread.CurrentThread.CurrentCulture.Name));
        }

        public void RegisterProcessor(IResourceProcessor processor)
        {
            this.processors.Add(processor);
        }


        /// <summary>
        /// Gets the resources in correct order.
        /// </summary>
        public IEnumerable<ResourceBase> GetResourcesInOrder()
        {
            if (processors.Count == 0 && configuration.Resources.DefaultResourceProcessors.Count == 0)
                return requiredResourcesOrdered.Select(k => requiredResources[k]);
            return GetNamedResourcesInOrder().Select(r => r.Resource);
        }
        /// <summary>
        /// Gets the resources with name in correct order.
        /// </summary>
        public IEnumerable<NamedResource> GetNamedResourcesInOrder()
        {
            var result = requiredResourcesOrdered.Select(k => new NamedResource(k, requiredResources[k]));

            foreach (var proc in configuration.Resources.DefaultResourceProcessors)
            {
                result = proc.Process(result);
            }
            foreach (var proc in processors)
            {
                result = proc.Process(result);
            }
            return result;
        }


        /// <summary>
        /// Finds the resource in required resources or in the resources registered in the configuration file.
        /// </summary>
        public ResourceBase FindResource(string name)
        {
            ResourceBase resource;
            if (requiredResources.TryGetValue(name, out resource))
            {
                return resource;
            }

            resource = configuration.Resources.FindResource(name);
            if (resource == null)
            {
                ThrowResourceNotFound(name);
            }

            return resource;
        }

        private static void ThrowNonUniqueName(string name)
        {
            throw new ArgumentException(string.Format("Different resource with the same name '{0}' is already registered!", name));
        }

        private static void ThrowResourceNotFound(string name)
        {
            throw new ArgumentException(string.Format("The resource '{0}' could not be found. Make sure it is registered in the dotvvm.json file or in the startup class.", name));
        }


    }
}
