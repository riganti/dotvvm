using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Utils;
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
        private const string HashParameterName = "hash";
        private const string NameParameterName = "name";
        private const string LocationQueryName = "location";

        private readonly IResourceHashService hasher;
        private readonly RouteBase resourceRoute;
        private readonly DotvvmResourceRepository resources;
        private readonly ConcurrentDictionary<string, string> alternateDirectories;
        private readonly bool suppressVersionHash;

        public LocalResourceUrlManager(DotvvmConfiguration configuration, IResourceHashService hasher)
        {
            this.resourceRoute = new DotvvmRoute(
                url: $"{HostingConstants.ResourceRouteName}/{{{HashParameterName}}}/{{{NameParameterName}:regex(.*)}}",
                virtualPath: null,
                defaultValues: null,
                presenterFactory: null,
                configuration: configuration);
            this.hasher = hasher;
            this.resources = configuration.Resources;
            this.alternateDirectories = configuration.Debug ? new ConcurrentDictionary<string, string>() : null;
            this.suppressVersionHash = configuration.Debug;
        }

        public string GetResourceUrl(ILocalResourceLocation resource,
            IDotvvmRequestContext context,
            string name)
        {
            var locationOwner = resources.FindResource(name);
            var url = resourceRoute.BuildUrl(new Dictionary<string, object> {
                [HashParameterName] = GetVersionHash(resource, context, name),
                [NameParameterName] = EncodeResourceName(name)
            });
            if (locationOwner is ILinkResource link)
            {
                var locations = link.GetLocations().ToArray();
                var locationIndex = Array.IndexOf(locations, resource);
                if (locationIndex != 0)
                {
                    url += UrlHelper.BuildUrlSuffix(null, new Dictionary<string, object> {
                        [LocationQueryName] = locationIndex
                    });
                }
            }
            return url;
        }

        protected virtual string EncodeResourceName(string name)
        {
            return name.Replace(":", "---").Replace(".", "--");
        }

        protected virtual string DecodeResourceName(string name)
        {
            return name.Replace("---", ":").Replace("--", ".");
        }

        protected virtual string GetVersionHash(ILocalResourceLocation location, IDotvvmRequestContext context, string name) =>
            suppressVersionHash ? // don't generate the hash iff !Debug, as it clears breakpoints in debugger when url changes
            EncodeResourceName(name) :
            hasher.GetVersionHash(location, context);

        public ILocalResourceLocation FindResource(string url, IDotvvmRequestContext context, out string mimeType)
        {
            mimeType = null;
            if (DotvvmRoutingMiddleware.FindMatchingRoute(new[] { resourceRoute }, context, out var parameters) == null)
            {
                return null;
            }

            var name = DecodeResourceName((string)parameters[NameParameterName]);
            var hash = (string)parameters[HashParameterName];
            int? locationIndex = null;
            if (context.Query.TryGetValue(LocationQueryName, out var locationString))
            {
                locationIndex = (int)ReflectionUtils.ConvertValue(locationString, typeof(int));
            }
            if (resources.FindResource(name) is IResource resource)
            {
                var location = FindLocation(resource, locationIndex, out mimeType);
                if (GetVersionHash(location, context, name) == hash) // check if the resource matches so that nobody can gues the url by chance
                {
                    if (alternateDirectories != null)
                    {
                        alternateDirectories.GetOrAdd(hash, _ => (location as IDebugFileLocalLocation)?.GetFilePath(context));
                    }

                    return location;
                }
            }

            return TryLoadAlternativeFile(name, hash, context);
        }

        private ILocalResourceLocation TryLoadAlternativeFile(string name, string hash, IDotvvmRequestContext context)
        {
            if (alternateDirectories != null && alternateDirectories.TryGetValue(hash, out string filePath) && filePath != null)
            {
                var directory = Path.GetDirectoryName(Path.Combine(context.Configuration.ApplicationPhysicalPath, filePath));
                if (directory != null)
                {
                    var sourceFile = Path.Combine(directory, name);
                    if (File.Exists(sourceFile))
                    {
                        return new FileResourceLocation(sourceFile);
                    }
                }
            }
            return null;
        }

        protected ILocalResourceLocation FindLocation(
            IResource resource,
            int? locationIndex,
            out string mimeType)
        {
            if (!(resource is ILinkResource link))
            {
                mimeType = null;
                return null;
            }

            mimeType = link.MimeType;
            var locations = link.GetLocations().ToArray();
            if (locationIndex == null)
            {
                return locations
                    .OfType<ILocalResourceLocation>()
                    .FirstOrDefault();
            }
            // unless the order of locations changes, the location has to be local
            return (ILocalResourceLocation)locations.ElementAtOrDefault(locationIndex.Value);
        }
    }
}
