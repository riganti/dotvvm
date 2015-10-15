using DotVVM.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Routing
{
    public static class RouteHelper
    {
        private static HashSet<string> defaultFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Index", "Default" };
        private static IEnumerable<string> GetRoutesForFile(string fileName)
        {
            var directory = Path.GetDirectoryName(fileName);
            var pureName = Path.GetFileNameWithoutExtension(fileName);

            if (!char.IsLetterOrDigit(pureName[0])) yield break;

            if (defaultFiles.Contains(pureName)) yield return directory;

            if (!string.IsNullOrEmpty(directory)) yield return directory + "/" + pureName;
            else yield return directory + pureName; 
        }



        public static void AutoRegisterRoutes(this DotvvmConfiguration config, string path = "/", string pattern = "*.dothtml")
            => AutoRegisterRoutes(config, GetRoutesForFile, path, pattern);
        public static void AutoRegisterRoutes(this DotvvmConfiguration config, Func<string, IEnumerable<string>> getRouteList, string path = "/", string pattern = "*.dothtml")
        {
            var rootPath = config.ApplicationPhysicalPath.Replace('\\','/').TrimEnd('/') + "/" +  path.Replace('\\','/').TrimStart('/');

            if (!rootPath.EndsWith("/", StringComparison.Ordinal)) rootPath += "/";

            var mappedFiles = new HashSet<string>(config.RouteTable.Select(r => r.VirtualPath));

            foreach (var filePath in Directory.EnumerateFiles(rootPath, pattern, SearchOption.AllDirectories))
            {
                var virtualPath = filePath.Substring(rootPath.Length);
                if (mappedFiles.Contains(virtualPath)) continue;
                var routes = getRouteList(virtualPath).ToArray();
                foreach (var route in routes)
                {
                    config.RouteTable.Add(route, route, virtualPath);
                }
            }
        }
    }
}
