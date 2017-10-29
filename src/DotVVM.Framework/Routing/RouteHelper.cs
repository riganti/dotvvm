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
        public static void AutoRegisterRoutes(this DotvvmRouteTable routeTable, DotvvmConfiguration configuration, string path = "", string pattern = "*.dothtml") =>
            routeTable.AutoRegisterRoutes(configuration, null, path, pattern);
        public static void AutoRegisterRoutes(this DotvvmConfiguration config, string path = "", string pattern = "*.dothtml") =>
            config.RouteTable.AutoRegisterRoutes(config, null, path, pattern);

        public static void AutoRegisterRoutes(this DotvvmConfiguration config, Func<string, IEnumerable<string>> getRouteList, string path = "/", string pattern = "*.dothtml") =>
            config.RouteTable.AutoRegisterRoutes(config, getRouteList, path, pattern);
        public static void AutoRegisterRoutes(this DotvvmRouteTable routeTable, DotvvmConfiguration config, Func<string, IEnumerable<string>> getRouteList, string path = "/", string pattern = "*.dothtml") =>
            routeTable.AutoDiscoverRoutes(new CustomListRoutingStrategy(config, getRouteList, path, pattern));

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