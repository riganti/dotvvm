using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotVVM.Framework.Routing
{
    /// <summary>
    /// A default route discover strategy which browses the Views folder and automatically creates routes for all *.dothtml files.
    /// </summary>
    public class DefaultRouteStrategy : IRoutingStrategy
    {

        private readonly DotvvmConfiguration configuration;
        private readonly string applicationPhysicalPath;
        private readonly DirectoryInfo viewsFolderDirectoryInfo;


        public DefaultRouteStrategy(DotvvmConfiguration configuration, string viewsFolder = "Views")
        {
            this.configuration = configuration;
            this.applicationPhysicalPath = Path.GetFullPath(configuration.ApplicationPhysicalPath);

            var directory = Path.Combine(applicationPhysicalPath, viewsFolder);
            viewsFolderDirectoryInfo = new DirectoryInfo(directory);
        }


        public virtual IEnumerable<RouteBase> GetRoutes()
        {
            var existingRouteNames = new HashSet<string>(configuration.RouteTable.Select(r => r.RouteName), StringComparer.OrdinalIgnoreCase);

            return DiscoverMarkupFiles()
                .Select(BuildRoute)
                .Where(r => !existingRouteNames.Contains(r.RouteName));
        }

        /// <summary>
        /// Discovers all markup files for which the route should be created.
        /// </summary>
        protected virtual IEnumerable<RouteStrategyMarkupFileInfo> DiscoverMarkupFiles()
        {
            if (!viewsFolderDirectoryInfo.Exists)
            {
                throw new DotvvmRouteStrategyException($"Cannot auto-discover DotVVM routes. The directory '{viewsFolderDirectoryInfo.FullName}' was not found!");
            }

            return viewsFolderDirectoryInfo
                .GetFiles("*.dothtml", SearchOption.AllDirectories)
                .Select(file => new RouteStrategyMarkupFileInfo()
                {
                    AbsolutePath = file.FullName,
                    AppRelativePath = file.FullName.Substring(applicationPhysicalPath.Length).Replace('\\', '/').TrimStart('/'),
                    ViewsFolderRelativePath = file.FullName.Substring(viewsFolderDirectoryInfo.FullName.Length).Replace('\\', '/').TrimStart('/')
                });
        }

        /// <summary>
        /// Builds a route for the specified markup file.
        /// </summary>
        protected virtual RouteBase BuildRoute(RouteStrategyMarkupFileInfo file)
        {
            var routeName = GetRouteName(file);
            var url = GetRouteUrl(file);
            var defaultParameters = GetRouteDefaultParameters(file);
            var presenterFactory = GetRoutePresenterFactory(file);

            return new DotvvmRoute(url, file.AppRelativePath, defaultParameters, presenterFactory, configuration)
            {
                RouteName = routeName
            };
        }

        protected virtual string GetRouteName(RouteStrategyMarkupFileInfo file)
        {
            return GetRouteUrl(file).Replace('/', '_');
        }

        protected virtual string GetRouteUrl(RouteStrategyMarkupFileInfo file)
        {
            var pathWithoutExtension = file.ViewsFolderRelativePath.Substring(0, file.ViewsFolderRelativePath.Length - ".dothtml".Length);
            return pathWithoutExtension;
        }

        protected virtual object GetRouteDefaultParameters(RouteStrategyMarkupFileInfo file)
        {
            return null;
        }

        protected virtual Func<IDotvvmPresenter> GetRoutePresenterFactory(RouteStrategyMarkupFileInfo file)
        {
            return configuration.RouteTable.GetDefaultPresenter;
        }
    }
}