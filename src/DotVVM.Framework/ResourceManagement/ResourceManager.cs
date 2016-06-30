using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Configuration;
using System.Threading;
using System.Web.Hosting;
using System.Web.Optimization;
using DotVVM.Framework.Compilation.Parser;

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
        private List<string> requiredResourcesForBundle = new List<string>();
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
            var bundleName = configuration.Resources.GetBundleName(name);
            if (bundleName != null)
            {
                if (!requiredResourcesOrdered.Contains(bundleName) && !requiredResourcesForBundle.Contains(name))
                    AddScriptBundle(bundleName);
                return;
            }

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
            AddRequiredResource(string.Format(ResourceConstants.GlobalizeCultureResourceName, Thread.CurrentThread.CurrentCulture.Name));
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


        private void AddScriptBundle(string bundleName)
        {
            List<string> orderedResourcesForBundle = new List<string>();
            foreach (var resourceName in configuration.Resources.ResourceBundleNames.FirstOrDefault(b => b.Key.Key == bundleName).Value)
            {
                AddResourceToBundle(resourceName, ref orderedResourcesForBundle);
            }

            var bundleSuffix =
                configuration.Resources.ResourceBundleNames.FirstOrDefault(b => b.Key.Key == bundleName).Key.Value;

            BundleTable.Bundles.FileExtensionReplacementList.Clear();
            BundleTable.Bundles.IgnoreList.Clear();
            AddDefaultIgnorePatterns(BundleTable.Bundles.IgnoreList);

            var scriptForBundle = new ScriptBundle("~/" + typeof(DotvvmConfiguration).Assembly.GetName().Version + "/" + bundleName + "/" + bundleSuffix);
            foreach (var resourceName in orderedResourcesForBundle)
            {
                var resource = configuration.Resources.FindResource(resourceName);
                if (resource is ScriptResource)
                {
                    ScriptResource scriptResource = resource as ScriptResource;
                    HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedVirtualPathProvider());
                    BundleTable.VirtualPathProvider = HostingEnvironment.VirtualPathProvider;

                    if (scriptResource.IsEmbeddedResource)
                    {

                        if (scriptResource.EmbeddedResourceAssembly.Contains("Bootstrap"))
                        {
                            scriptForBundle.Include("~/DotVVM.Framework.Controls.Bootstrap.dll/" + scriptResource.Url);
                            requiredResourcesForBundle.Add(resourceName);
                        }
                        else
                        {
                            scriptForBundle.Include("~/DotVVM.Framework.dll/" + scriptResource.Url);
                            requiredResourcesForBundle.Add(resourceName);
                        }
                    }
                    else
                    {
                        scriptForBundle.Include(scriptResource.Url);
                        requiredResourcesForBundle.Add(resourceName);
                    }
                }

                if (resource is StylesheetResource)
                {
                    AddRequiredResource(resourceName);
                }

                if (resource is InlineScriptResource)
                {
                    if (resourceName == "dotvvm")
                    {
                        scriptForBundle.Include("~/DotVVM.Framework.dll/DotVVM.Framework.Resources.Scripts.DotVVM.Declaration.js");
                        requiredResourcesForBundle.Add(resourceName);
                    }
                }

            }

            DisableMinificationIfDebug(ref scriptForBundle);

            BundleTable.Bundles.Add(scriptForBundle);


            AddRequiredScriptFile(bundleName, scriptForBundle.Path);
        }


        private void AddResourceToBundle(string resourceName, ref List<string> orderedResourcesListForBundle)
        {
            if (!orderedResourcesListForBundle.Contains(resourceName))
            {
                var resource = configuration.Resources.FindResource(resourceName);
                foreach (var dependency in resource.Dependencies)
                {
                    AddResourceToBundle(dependency, ref orderedResourcesListForBundle);
                }

                orderedResourcesListForBundle.Add(resourceName);
            }
        }

        public static void AddDefaultIgnorePatterns(IgnoreList ignoreList)
        {
            if (ignoreList == null)
                throw new ArgumentNullException("ignoreList");
            ignoreList.Ignore("*.intellisense.js");
            ignoreList.Ignore("*-vsdoc.js");
            ignoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
            //ignoreList.Ignore("*.min.js", OptimizationMode.WhenDisabled);
            ignoreList.Ignore("*.min.css", OptimizationMode.WhenDisabled);
        }


        [Conditional("DEBUG")]
        private static void DisableMinificationIfDebug(ref ScriptBundle scriptForBundle)
        {
            scriptForBundle.Transforms.Clear();
        }


        private static void ThrowNonUniqueName(string name)
        {
            throw new ArgumentException($"Different resource with the same name '{name}' is already registered!");
        }

        private static void ThrowResourceNotFound(string name)
        {
            throw new ArgumentException($"The resource '{name}' could not be found. Make sure it is registered in the startup class.");
        }


    }
}
