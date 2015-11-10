using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotVVM.Framework.Routing
{
    public class FolderBasedRouteStrategy : IRoutingStrategy
    {
        protected string VirtualViewsFolderPath { get; set; }
        protected bool TryDetermineDefaultRoute { get; set; }
        protected DirectoryInfo ViewFolder { get; set; }
        protected IList<string> OrderedDefaultStartupDocuments { get; set; }

        /// <summary>
        /// Provides collection of routes created from file syststem.
        /// </summary>
        public FolderBasedRouteStrategy(DotvvmConfiguration configuration, string virtualViewsFolderPath, bool tryDetermineDefaultRoute)
        {
            VirtualViewsFolderPath = virtualViewsFolderPath;
            TryDetermineDefaultRoute = tryDetermineDefaultRoute;

            OrderedDefaultStartupDocuments = new List<string> { "Default", "Index", "AppStart", "IisStart" };

            if (!virtualViewsFolderPath.StartsWith(configuration.ApplicationPhysicalPath))
            {
                virtualViewsFolderPath = Path.Combine(configuration.ApplicationPhysicalPath, virtualViewsFolderPath);
            }

            var dir = new DirectoryInfo(virtualViewsFolderPath);
            if (!dir.Exists)
            {
                throw new DotvvmRouteStrategyException($"Routing strategy cannot find path {virtualViewsFolderPath}");
            }
            ViewFolder = dir;
        }

        /// <summary>
        /// Returns collection of routes created by strategy.
        /// </summary>
        public virtual IEnumerable<RouteInfo> GetRoutes()
        {
            var routes = new List<RouteInfo>();
            var files = ViewFolder.GetFiles($"*{MarkupFile.ViewFileExtension}", SearchOption.AllDirectories).ToList();
            var charactersCountToRemove = (ViewFolder.FullName + (ViewFolder.FullName.EndsWith("/") ? "" : "/")).Length;
            var defaultFound = false;
            var defaultDocumentNameIndex = -1;

            files.ForEach(f =>
            {
                var virtualPath = f.FullName.Remove(0, charactersCountToRemove).Replace("\\", "/");
                var url = virtualPath.Remove(virtualPath.Length - MarkupFile.ViewFileExtension.Length, MarkupFile.ViewFileExtension.Length);
                var name = url.Replace("/", "_");

                routes.Add(new RouteInfo()
                {
                    RouteName = name,
                    RouteUrl = url,
                    VirtualPath = Combine(VirtualViewsFolderPath, virtualPath)
                });
                //determine default route
                if (!defaultFound && TryDetermineDefaultRoute)
                {
                    //get index of matching default document
                    var defaultDocument =
                    Enumerable.Range(0, OrderedDefaultStartupDocuments.Count)
                        .Where(
                            i =>
                                string.Equals(OrderedDefaultStartupDocuments[i], name,
                                    StringComparison.OrdinalIgnoreCase))
                        .Select(i => (int?)i)
                        .FirstOrDefault();

                    if (defaultDocument != null)
                    {
                        // check priority
                        if (defaultDocumentNameIndex == -1 || defaultDocumentNameIndex > (defaultDocument ?? 0))
                        {
                            var defaultRoute = CopyRouteInfo(routes[routes.Count - 1]);
                            defaultRoute.RouteUrl = "";
                            defaultRoute.RouteName = DefaultRouteName;
                            if (defaultDocumentNameIndex != -1)
                            {
                                routes.Remove(routes.Find(s => s.RouteName == DefaultRouteName));
                            }
                            routes.Add(defaultRoute);
                            defaultDocumentNameIndex = defaultDocument ?? 0;
                        }

                        if (defaultDocument == 0)
                        {
                            defaultFound = true;
                        }
                    }
                }
            });

            return routes;
        }

        /// <summary>
        /// If starategy creates default route, value of this property is used as name of the route.
        /// </summary>
        public string DefaultRouteName { get; set; } = "DefaultRoute";

        /// <summary>
        /// Creates copy of provided RouteInfo instance.
        /// </summary>
        protected RouteInfo CopyRouteInfo(RouteInfo info)
        {
            return new RouteInfo() { RouteUrl = info.RouteUrl, VirtualPath = info.VirtualPath, RouteName = info.RouteName, DefaultObject = info.DefaultObject };
        }

        /// <summary>
        /// Combines to parts of url.
        /// </summary>
        private string Combine(string a, string b)
        {
            if (a.Length == 0) return b;
            if (b.Length == 0) return a;

            if (b.StartsWith("/"))
            {
                b = b.Substring(1);
            }

            if (a[a.Length - 1] == '/')
            {
                return a + b;
            }
            return a + "/" + b;
        }
    }
}