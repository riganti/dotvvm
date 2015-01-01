using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Configuration;

namespace Redwood.Framework.Hosting
{
    /// <summary>
    /// Holds all required resources and render them to the page in correct order.
    /// </summary>
    public class ResourceManager
    {
        private readonly RedwoodConfiguration configuration;
        private Dictionary<string, ResourceBase> requiredResources = new Dictionary<string, ResourceBase>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceManager"/> class.
        /// </summary>
        public ResourceManager(RedwoodConfiguration configuration)
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

        /// <summary>
        /// Adds the resource and checks name conflicts.
        /// </summary>
        private void AddRequiredResourceCore(string name, ResourceBase resource)
        {
            ResourceBase originalResource;
            if (requiredResources.TryGetValue(name, out originalResource))
            {
                if (originalResource != resource)
                {
                    ThrowNonUniqueName(name);
                }
            }
            else
            {
                requiredResources[name] = resource;
            }
        }

        /// <summary>
        /// Adds the required script file.
        /// </summary>
        public void AddRequiredScriptFile(string name, string url, params string[] dependentResourceNames)
        {
            AddRequiredResourceCore(name, new ScriptResource() { Name = name, Url = url, Dependencies = dependentResourceNames });
        }

        /// <summary>
        /// Adds the required stylesheet file.
        /// </summary>
        public void AddRequiredStylesheetFile(string name, string url, params string[] dependentResourceNames)
        {
            AddRequiredResourceCore(name, new StylesheetResource() { Name = name, Url = url, Dependencies = dependentResourceNames });
        }

        /// <summary>
        /// Adds the specified piece of javascript that will be executed when the page is loaded.
        /// </summary>
        public void AddStartupScript(string name, string javascriptCode, params string[] dependentResourceNames)
        {
            AddRequiredResourceCore(name, new InlineScriptResource() { Name = name, Code = javascriptCode, Dependencies = dependentResourceNames });
        }

        /// <summary>
        /// Gets the resources in correct order.
        /// </summary>
        public IEnumerable<ResourceBase> GetResourcesInCorrectOrder()
        {
            var outputResources = new List<ResourceBase>();
            var outputResourceNames = new HashSet<string>();

            foreach (var resource in requiredResources.Values)
            {
                AddResourceWithDependencies(resource, outputResourceNames, outputResources);
            }

            return outputResources;
        }

        /// <summary>
        /// Adds the resource with dependencies.
        /// </summary>
        private void AddResourceWithDependencies(ResourceBase resource, HashSet<string> outputResourceNames, List<ResourceBase> outputResources)
        {
            // at first, add all dependencies
            foreach (var dependency in resource.Dependencies)
            {
                if (!outputResourceNames.Contains(dependency))
                {
                    var dependentResource = FindResource(dependency);
                    AddResourceWithDependencies(dependentResource, outputResourceNames, outputResources);
                }
            }

            // then, add the resource itself
            if (!outputResourceNames.Contains(resource.Name))
            {
                outputResources.Add(resource);
                outputResourceNames.Add(resource.Name);
            }
        }

        /// <summary>
        /// Finds the resource in required resources or in the resources registered in the configuration file.
        /// </summary>
        private ResourceBase FindResource(string name)
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
            throw new ArgumentException(string.Format("The resource '{0}' could not be found. Make sure it is registered in the redwood.json file or in the startup class.", name));
        }


    }
}
