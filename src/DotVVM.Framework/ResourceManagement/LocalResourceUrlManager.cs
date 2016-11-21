using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public class LocalResourceUrlManager : ILocalResourceUrlManager
    {
        private readonly IResourceHashService hasher;
        private readonly RouteBase resourceRoute;
        private readonly DotvvmResourceRepository resources;

        public LocalResourceUrlManager(DotvvmConfiguration configuration, IResourceHashService hasher)
        {
            this.resourceRoute = new DotvvmRoute("dotvvmResource/{hash}/{name}", null, null, null, configuration);
            this.hasher = hasher;
            this.resources = configuration.Resources;
        }

        public string GetResourceUrl(ILocalResourceLocation resource, IDotvvmRequestContext context, string name) =>
            resourceRoute.BuildUrl(new Dictionary<string, object>
            {
                ["hash"] = hasher.GetVersionHash(resource, context),
                ["name"] = name
            });

        public ILocalResourceLocation FindResource(string url, IDotvvmRequestContext context, out string mimeType)
        {
            mimeType = null;
            IDictionary<string, object> parameters;
            if (DotvvmRoutingMiddleware.FindMatchingRoute(new[] { resourceRoute }, context, out parameters) == null) return null;
            var name = (string)parameters["name"];
            var hash = (string)parameters["hash"];
            var resource = resources.FindResource(name) as IResource;
            if (resource == null) return null;
            var location = FindLocation(resource, out mimeType);
            if (hasher.GetVersionHash(location, context) != hash) return null; // check if the resource matches so that nobody can gues the url by chance
            return location;
        }

        protected ILocalResourceLocation FindLocation(IResource resource, out string mimeType)
        {
            var linkResource = resource as ILinkResource;
            mimeType = linkResource?.MimeType;
            return linkResource
                   ?.GetLocations()
                   ?.OfType<ILocalResourceLocation>()
                   ?.FirstOrDefault();
        }
    }
}
