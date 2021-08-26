#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Configuration;
using System.Threading;
using DotVVM.Framework.Compilation.Parser;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ResourceManagement
{
    /// <summary>
    /// Holds all required resources and render them to the page in correct order.
    /// </summary>
    public class ResourceManager
    {
        private readonly List<string> requiredResourcesOrdered = new List<string>();
        private readonly Dictionary<string, IResource> requiredResources = new Dictionary<string, IResource>();
        private readonly List<IResourceProcessor> processors = new List<IResourceProcessor>();
        private int nonameCtr = 0;
        private readonly DotvvmResourceRepository repository;

        public IReadOnlyCollection<string> RequiredResources
        {
            get { return requiredResourcesOrdered.AsReadOnly(); }
        }

        internal bool HeadRendered;
        internal bool BodyRendered;
        private HashSet<string> renderedResources = new HashSet<string>();


        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceManager"/> class.
        /// </summary>
        public ResourceManager(DotvvmResourceRepository repository)
        {
            this.repository = repository;
        }

        internal void MarkRendered(NamedResource resource)
        {
            renderedResources.Add(resource.Name);
            switch (resource.Resource.RenderPosition)
            {
                case ResourceRenderPosition.Head:
                    Debug.Assert(HeadRendered);
                    break;
                case ResourceRenderPosition.Body:
                    Debug.Assert(BodyRendered);
                    break;
                case ResourceRenderPosition.Anywhere:
                    Debug.Assert(HeadRendered);
                    break;
            }
        }

        public bool IsRendered(string resourceName) => renderedResources.Contains(resourceName);

        /// <summary>
        /// Adds the required resource with specified name.
        /// </summary>
        public void AddRequiredResource(string name)
        {
            var resource = repository.FindResource(name);
            if (resource == null)
            {
                ThrowResourceNotFound(name);
            }

            AddRequiredResource(name, resource!);
        }

        /// <summary>
        /// Adds the template resource at the end of the HTML document.
        /// </summary>
        /// <param name="template">The rendered DOM elements.</param>
        /// <returns>Resource ID</returns>
        public string AddTemplateResource(string template)
        {
            var resourceId = HashUtils.HashAndBase64Encode(template);
            if (!requiredResources.ContainsKey(resourceId))
            {
                AddRequiredResource(resourceId, new TemplateResource(template));
            }

            return resourceId;
        }
        /// <summary>
        /// Adds the resource with unique name.
        /// </summary>
        /// <param name="resource"></param>
        public void AddRequiredResource(IResource resource) => AddRequiredResource("__noname_" + nonameCtr++, resource);

        /// <summary>
        /// Adds the resource and checks name conflicts.
        /// </summary>
        public void AddRequiredResource(string name, IResource resource)
        {
            if (requiredResources.TryGetValue(name, out var originalResource))
            {
                if (originalResource != resource)
                {
                    ThrowNonUniqueName(name);
                }
            }
            else
            {
                if (this.IsAlreadyRendered(resource.RenderPosition))
                    throw new Exception($"Can't add {resource.GetType().Name} '{name}' to {resource.RenderPosition}, it is already rendered.");
                ResourceUtils.AssertAcyclicDependencies(resource, name, FindResource);
                foreach (var dep in resource.Dependencies)
                {
                    AddRequiredResource(dep);
                }
                requiredResourcesOrdered.Add(name);
                requiredResources[name] = resource;
            }
        }

        /// Checks whether the resource position is already rendered.
        private bool IsAlreadyRendered(ResourceRenderPosition position) =>
            position == ResourceRenderPosition.Head && HeadRendered ||
            (position == ResourceRenderPosition.Body || position == ResourceRenderPosition.Anywhere) && BodyRendered;

        /// <summary>
        /// Adds the required script file.
        /// </summary>
        public void AddRequiredScriptFile(string name, string url, params string[] dependentResourceNames)
        {
            var defer = dependentResourceNames.Any(IsDeferred);
            AddRequiredResource(name, new ScriptResource(defer: defer, location: CreateRelativeResourceLocation(url)) {
                Dependencies = dependentResourceNames,
            });
        }

        private static IResourceLocation CreateRelativeResourceLocation(string url)
        {
            return url.StartsWith("~/", StringComparison.Ordinal) ?
                   new FileResourceLocation(url.Substring(2)) :
                   (IResourceLocation)new UrlResourceLocation(url);
        }

        /// <summary>
        /// Adds the required stylesheet file.
        /// </summary>
        public void AddRequiredStylesheetFile(string name, string url, params string[] dependentResourceNames)
        {
            AddRequiredResource(name, new StylesheetResource(CreateRelativeResourceLocation(url)) {
                Dependencies = dependentResourceNames,
            });
        }

        bool IsDeferred(string name) => this.FindResource(name) is IDeferrableResource r && r.Defer;

        /// <summary>
        /// Adds the specified piece of javascript that will be executed when the page is loaded.
        /// </summary>
        public void AddStartupScript(string name, string javascriptCode, params string[] dependentResourceNames)
        {
            var defer = dependentResourceNames.Any(IsDeferred);
            AddRequiredResource(name, new InlineScriptResource(javascriptCode, defer: defer) { Dependencies = dependentResourceNames });
        }

        /// <summary>
        /// Adds the specified piece of javascript that will be executed when the page is loaded.
        /// </summary>
        public void AddStartupScript(string name, string javascriptCode, bool defer, params string[] dependentResourceNames)
        {
            AddRequiredResource(name, new InlineScriptResource(javascriptCode, defer: defer) { Dependencies = dependentResourceNames });
        }

        /// <summary>
        /// Adds the specified piece of javascript that will be executed when the page is loaded.
        /// </summary>
        public void AddStartupScript(string javascriptCode, params string[] dependentResourceNames)
        {
            var defer = dependentResourceNames.Any(IsDeferred);
            AddRequiredResource(new InlineScriptResource(javascriptCode, defer: defer) { Dependencies = dependentResourceNames });
        }

        /// <summary>
        /// Adds the specified piece of javascript that will be executed when the page is loaded.
        /// </summary>
        public void AddStartupScript(string javascriptCode, bool defer, params string[] dependentResourceNames)
        {
            AddRequiredResource(new InlineScriptResource(javascriptCode, defer: defer) { Dependencies = dependentResourceNames });
        }

        /// <summary> Adds a script tag with inline content. The script is identified by the hash of it's content, so it will be in the page only once. </summary>
        public string AddInlineScript(string javascriptCode, params string[] dependentResourceNames)
        {
            var resourceId = "iscr_" + HashUtils.HashAndBase64Encode(javascriptCode);
            if (!requiredResources.ContainsKey(resourceId))
            {
                var defer = dependentResourceNames.Any(IsDeferred);
                AddRequiredResource(resourceId, new InlineScriptResource(javascriptCode, defer: defer) { Dependencies = dependentResourceNames });
            }

            return resourceId;

        }

        /// <summary>
        /// Adds the globalization file for current thread culture.
        /// </summary>
        public void AddCurrentCultureGlobalizationResource()
        {
            AddRequiredResource(string.Format(ResourceConstants.GlobalizeCultureResourceName, CultureInfo.CurrentCulture.Name));
        }

        public void RegisterProcessor(IResourceProcessor processor)
        {
            this.processors.Add(processor);
        }


        /// <summary>
        /// Gets the resources in correct order.
        /// </summary>
        public IEnumerable<IResource> GetResourcesInOrder()
        {
            if (processors.Count == 0 && repository.DefaultResourceProcessors.Count == 0)
                return requiredResourcesOrdered.Select(k => requiredResources[k]);
            return GetNamedResourcesInOrder().Select(r => r.Resource);
        }
        /// <summary>
        /// Gets the resources with name in correct order.
        /// </summary>
        public IEnumerable<NamedResource> GetNamedResourcesInOrder()
        {
            var result = requiredResourcesOrdered.Select(k => new NamedResource(k, requiredResources[k]));

            foreach (var proc in repository.DefaultResourceProcessors)
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
        /// Finds a resource in required resources or in the resources registered in the configuration file. Throws an exception if the resource is not found.
        /// </summary>
        public IResource FindResource(string name)
        {
            IResource? resource;
            if (requiredResources.TryGetValue(name, out resource))
            {
                return resource;
            }

            resource = repository.FindResource(name);
            if (resource == null)
            {
                ThrowResourceNotFound(name);
            }

            return resource!;
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
