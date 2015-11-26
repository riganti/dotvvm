using DotVVM.Framework.Configuration;
using DotVVM.Framework.ViewModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotVVM.Framework.Routing
{
    public static class RouteHelper
    {
        private static HashSet<string> defaultFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Index", "Default" };

        private static IEnumerable<string> GetRoutesForFile(string fileName)
        {
            var slashIndex = fileName.LastIndexOf('/');
            var pureName = Path.GetFileNameWithoutExtension(slashIndex < 0 ? fileName : fileName.Substring(slashIndex + 1));
            var directory = slashIndex < 0 ? "" : fileName.Remove(slashIndex);

            if (!char.IsLetterOrDigit(pureName[0])) yield break;

            if (defaultFiles.Contains(pureName)) yield return directory;

            if (!string.IsNullOrEmpty(directory)) yield return directory + "/" + pureName;
            else yield return pureName;
        }

        [Obsolete("Use IRoutingStrategy.")] // TODO: make this a helper for IRoutingStrategy
        public static void AutoRegisterRoutes(this DotvvmConfiguration config, string path = "", string pattern = "*.dothtml")
            => AutoRegisterRoutes(config, GetRoutesForFile, path, pattern);

        [Obsolete("Use IRoutingStrategy.")]
        public static void AutoRegisterRoutes(this DotvvmConfiguration config, Func<string, IEnumerable<string>> getRouteList, string path = "/", string pattern = "*.dothtml")
        {
            path = path.Replace('\\', '/');
            if (path.StartsWith("/", StringComparison.Ordinal)) path = path.Remove(0, 1);
            var rootPath = config.ApplicationPhysicalPath.Replace('\\', '/').TrimEnd('/') + "/" + path;

            if (!rootPath.EndsWith("/", StringComparison.Ordinal)) rootPath += "/";

            var mappedFiles = new HashSet<string>(config.RouteTable.Select(r => r.VirtualPath));

            foreach (var filePath in Directory.EnumerateFiles(rootPath, pattern, SearchOption.AllDirectories))
            {
                var virtualPath = Combine(path, filePath.Substring(rootPath.Length).TrimStart('/', '\\').Replace('\\', '/'));
                if (mappedFiles.Contains(virtualPath)) continue;
                var routes = getRouteList(virtualPath).ToArray();
                foreach (var route in routes)
                {
                    config.RouteTable.Add(route, route, virtualPath);
                }
            }
        }

        /// <summary>
        /// Adds collection of routes defined by <see cref="IRoutingStrategy"/> to RouteTable.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="strategy">Object that provides list of routes.</param>
        public static void RegisterRoutingStrategy(this DotvvmRouteTable table, IRoutingStrategy strategy)
        {
            var routes = new List<RouteInfo>(strategy.GetRoutes() ?? new List<RouteInfo>());
            routes.ForEach(r => table.Add(r.RouteName, r.RouteUrl, r.VirtualPath, r.DefaultObject));
        }

        private static string Combine(string a, string b)
        {
            if (a.Length == 0) return b;
            if (b.Length == 0) return a;
            if (a[a.Length - 1] == '/')
            {
                return a + b;
            }
            else
            {
                return a + "/" + b;
            }
        }
    }
}