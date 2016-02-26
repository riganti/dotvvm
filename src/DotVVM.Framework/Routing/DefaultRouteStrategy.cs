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
        private readonly DirectoryInfo directoryInfo;


        public DefaultRouteStrategy(DotvvmConfiguration configuration, string viewsFolder = "Views")
        {
            this.configuration = configuration;
            var directory = Path.Combine(configuration.ApplicationPhysicalPath, viewsFolder);
            directoryInfo = new DirectoryInfo(directory);
            if (!directoryInfo.Exists)
            {
                throw new DotvvmRouteStrategyException($"Cannot auto-discover DotVVM routes. The directory '{directory}' was not found!");
            }
        }


        public IEnumerable<RouteBase> GetRoutes()
        {
            return DiscoverMarkupFiles()
                .Select(BuildRoute);
        }

        /// <summary>
        /// Discovers all markup files for which the route should be created.
        /// </summary>
        protected virtual IEnumerable<RouteStrategyMarkupFileInfo> DiscoverMarkupFiles()
        {
            return directoryInfo
                .GetFiles("*.dothtml", SearchOption.AllDirectories)
                .Select(file => new RouteStrategyMarkupFileInfo()
                {
                    AbsolutePath = file.FullName,
                    AppRelativePath = file.FullName.Substring(directoryInfo.FullName.Length).Replace('\\', '/').TrimStart('/')
                });
        }

        /// <summary>
        /// Builds a route for the specified markup file.
        /// </summary>
        protected virtual RouteBase BuildRoute(RouteStrategyMarkupFileInfo file)
        {
            var url = GetRouteUrl(file);
            var defaultParameters = GetRouteDefaultParameters(file);
            var presenterFactory = GetRoutePresenterFactory(file);

            return new DotvvmRoute(url, file.AppRelativePath, defaultParameters, presenterFactory)
            {
                RouteName = GetRouteName(file)
            };
        }

        protected virtual string GetRouteName(RouteStrategyMarkupFileInfo file)
        {
            return string.Join("_", file.AppRelativePath.Split('/'));
        }

        protected virtual string GetRouteUrl(RouteStrategyMarkupFileInfo file)
        {
            return file.AppRelativePath.Substring(0, file.AppRelativePath.Length - ".dothtml".Length);
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