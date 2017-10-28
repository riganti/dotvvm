using DotVVM.Framework.Configuration;
using DotVVM.Framework.ViewModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        /// <summary>
        /// Registers all routes discovered by specified <see cref="IRoutingStrategy"/> in the RouteTable.
        /// </summary>
        /// <param name="strategy">A strategy that provides list of routes.</param>
        /// <param name="table">A table of DotVVM routes by specified name.</param>
        public static void AutoDiscoverRoutes(this DotvvmRouteTable table, IRoutingStrategy strategy)
        {
            foreach (var route in strategy.GetRoutes())
            {
                table.Add(route.RouteName, route);
            }
        }
    }
}