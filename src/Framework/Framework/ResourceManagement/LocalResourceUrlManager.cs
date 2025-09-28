using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotVVM.Framework.ResourceManagement
{
    public class LocalResourceUrlManager : ILocalResourceUrlManager
    {
        private readonly ILogger logger;
        private readonly IResourceHashService hasher;
        private readonly RouteBase resourceRoute;
        private readonly DotvvmResourceRepository resources;
        private readonly bool allowDebugFileResources;
        private readonly bool requireResourceVersionHash;
        private readonly bool allowResourceVersionHash;

        public LocalResourceUrlManager(DotvvmConfiguration configuration, IResourceHashService hasher, ILogger<LocalResourceUrlManager> logger)
        {
            this.resourceRoute = new DotvvmRoute(
                url: HostingConstants.ResourceRouteName + "-{name}/{fileName}",
                virtualPath: "",
                name: $"_dotvvm_{nameof(LocalResourceUrlManager)}",
                defaultValues: null,
                presenterFactory: _ => throw new NotSupportedException(),
                configuration: configuration);
            this.hasher = hasher;
            this.logger = logger;
            this.resources = configuration.Resources;
            this.allowDebugFileResources = configuration.Runtime.AllowResourceMapFiles.Enabled ?? configuration.Debug;
            this.requireResourceVersionHash = configuration.Runtime.RequireResourceVersionHash.Enabled ?? !configuration.Debug;
            this.allowResourceVersionHash = configuration.Runtime.AllowResourceVersionHash.Enabled ?? !configuration.Debug;
        }

        public string GetResourceUrl(ILocalResourceLocation resource,
            IDotvvmRequestContext context,
            string name)
        {
            var encodedName = EncodeResourceName(name);

            if (allowResourceVersionHash)
            {
                var versionHash = GetVersionHash(resource, context, name);
                return context.TranslateVirtualPath($"~/{HostingConstants.ResourceRouteName}-{encodedName}/{encodedName}?v={versionHash}");
            }
            else
            {
                return context.TranslateVirtualPath($"~/{HostingConstants.ResourceRouteName}-{encodedName}/{encodedName}");
            }
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
            hasher.GetVersionHash(location, context);

        public ILocalResourceLocation? FindResource(string url, IDotvvmRequestContext context, out string? mimeType)
        {
            mimeType = null;

            if (DotvvmRoutingMiddleware.FindExactMatchRoute([ resourceRoute ], url, out var parameters) == null)
            {
                return null;
            }

            var requestedName = (string)parameters!["name"]!;
            var name = DecodeResourceName(requestedName);
            var fileName = (string)parameters["fileName"]!;
            var hash = (string?)context.Query["v"];
            if (resources.FindResource(name) is IResource resource &&
                FindLocation(resource, out mimeType) is ILocalResourceLocation location)
            {
                if (fileName == requestedName)
                {
                    if (requireResourceVersionHash && GetVersionHash(location, context, name) != hash)
                    {   // check if the resource matches so that nobody can guess the url by chance
                        logger.LogInformation("Requested resource {name} with hash '{hash}' does not match the expected hash '{expectedHash}' and the request was rejected.", name, hash, GetVersionHash(location, context, name));
                        return null;
                    }

                    return location;
                }
                if (allowDebugFileResources)
                    return TryLoadAlternativeFile(name, fileName, resource, context);
            }

            logger.LogInformation("Requested resource '{name}' not found.", name);
            return null;

        }

        private static bool IsAllowedFileName(string name)
        {
            if (name.StartsWith("."))
                return false;
            if (name.Contains('/') || name.Contains('\\'))
                return false;
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return false;

            return name.EndsWith(".map", StringComparison.OrdinalIgnoreCase);
        }

        private ILocalResourceLocation? TryLoadAlternativeFile(string name, string fileName, IResource resource, IDotvvmRequestContext context)
        {
            if (!IsAllowedFileName(fileName))
            {
                logger.LogWarning("Requested additional file '{fileName}' for resource {resourceName} is not allowed.", fileName, name);
                return null;
            }

            var locations = (resource as ILinkResource)?.GetLocations().OfType<IDebugFileLocalLocation>().ToArray() ?? [];

            foreach (var location in locations)
            {
                if (location.TryGetSourceMap(fileName) is {} sourceMap)
                    return sourceMap;
            }

            // Load something.map.js or anything from the same directory
            var resourceDirectories =
                locations
                    .Select(x => x.GetFilePath(context)).WhereNotNull()
                    .Select(filePath => Path.GetDirectoryName(Path.Combine(context.Configuration.ApplicationPhysicalPath, filePath))).WhereNotNull()
                    .Distinct();

            foreach (var directory in resourceDirectories ?? [])
            {
                var sourceFile = Path.Combine(directory, fileName);
                if (File.Exists(sourceFile))
                {
                    return new FileResourceLocation(sourceFile);
                }
            }
            return null;
        }

        protected ILocalResourceLocation? FindLocation(IResource resource, out string? mimeType)
        {
            if (!(resource is ILinkResource link))
            {
                mimeType = null;
                return null;
            }

            mimeType = link.MimeType;
            return link
                .GetLocations()
                .OfType<ILocalResourceLocation>()
                .FirstOrDefault();
        }
    }
}
