using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Routing;
using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, string> alternateDirectories;

        public LocalResourceUrlManager(DotvvmConfiguration configuration, IResourceHashService hasher)
        {
            // On OWIN request which have a dot in file name does not work, so we have to put here another name
            var owinHackSuffix = Type.GetType("Owin.AppBuilderExtensions, DotVVM.Framework.Hosting.Owin") != null ? "/{sanitizedName}" : "";
            this.resourceRoute = new DotvvmRoute("dotvvmResource/{hash}/{name:regex(.*)}" + owinHackSuffix, null, null, null, configuration);
            this.hasher = hasher;
            this.resources = configuration.Resources;
            this.alternateDirectories = configuration.Debug ? new ConcurrentDictionary<string, string>() : null;
        }

        public string GetResourceUrl(ILocalResourceLocation resource, IDotvvmRequestContext context, string name) =>
            resourceRoute.BuildUrl(new Dictionary<string, object>
            {
                ["hash"] = hasher.GetVersionHash(resource, context),
                ["name"] = name,
                ["sanitizedName"] = new string(name.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray())
            });

        public ILocalResourceLocation FindResource(string url, IDotvvmRequestContext context, out string mimeType)
        {
            mimeType = null;
            IDictionary<string, object> parameters;
            if (DotvvmRoutingMiddleware.FindMatchingRoute(new[] { resourceRoute }, context, out parameters) == null) return null;
            var name = (string)parameters["name"];
            var hash = (string)parameters["hash"];
            var resource = resources.FindResource(name) as IResource;
            if (resource != null)
            {
                var location = FindLocation(resource, out mimeType);
                if (hasher.GetVersionHash(location, context) == hash) // check if the resource matches so that nobody can gues the url by chance
                {
                    if (alternateDirectories != null)
                        alternateDirectories.GetOrAdd(hash, _ => (location as IDebugFileLocalLocation)?.GetFilePath(context));
                    return location;
                }
            }

            return TryLoadAlternativeFile(name, hash, context);
        }

        private ILocalResourceLocation TryLoadAlternativeFile(string name, string hash, IDotvvmRequestContext context)
        {
            string filePath;
            if (alternateDirectories != null && alternateDirectories.TryGetValue(hash, out filePath) && filePath != null)
            {
                var directory = Path.GetDirectoryName(Path.Combine(context.Configuration.ApplicationPhysicalPath, filePath));
                if (directory != null)
                {
                    var sourceFile = Path.Combine(directory, name);
                    if (File.Exists(sourceFile)) return new LocalFileResourceLocation(sourceFile);
                }
            }
            return null;
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
