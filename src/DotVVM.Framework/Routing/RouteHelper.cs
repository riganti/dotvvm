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
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
        /// <summary>
        /// Verify whether all provided virtual paths exist and required properties are set. 
        /// </summary>
        /// <exception cref="DotvvmConfigurationException">Throws exception when configuration has invalid registrations.</exception> 
        public static void AssertConfigurationIsValid(this DotvvmConfiguration config)
        {

            var invalidRoutes = new List<DotvvmConfigurationAssertResult<RouteBase>>();
            var invalidControls = new List<DotvvmConfigurationAssertResult<DotvvmControlConfiguration>>();
            var loader = config.ServiceProvider.GetRequiredService<IMarkupFileLoader>();

            foreach (var route in config.RouteTable.Where(s => !string.IsNullOrWhiteSpace(s.VirtualPath)))
            {
                if (string.IsNullOrWhiteSpace(route.RouteName))
                {
                    invalidRoutes.Add(new DotvvmConfigurationAssertResult<RouteBase>(route, DotvvmConfigurationAssertReason.MissingRouteName));
                }

                var content = loader.GetMarkup(config, route.VirtualPath);
                if (content == null)
                {
                    invalidRoutes.Add(new DotvvmConfigurationAssertResult<RouteBase>(route, DotvvmConfigurationAssertReason.MissingFile));
                }
            }
            foreach (var control in config.Markup.Controls)
            {

                if (string.IsNullOrEmpty(control.TagPrefix))
                {
                    throw new Exception("The TagPrefix must not be empty and must not contain non-alphanumeric characters!");       // TODO: exception handling
                }

                if (string.IsNullOrEmpty(control.TagName))
                {
                    if (!string.IsNullOrEmpty(control.Src) || string.IsNullOrEmpty(control.Namespace) || string.IsNullOrEmpty(control.Assembly))
                    {
                        invalidControls.Add(new DotvvmConfigurationAssertResult<DotvvmControlConfiguration>(control, DotvvmConfigurationAssertReason.InvalidCombination));
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(control.Src) || !string.IsNullOrEmpty(control.Namespace) || !string.IsNullOrEmpty(control.Assembly))
                    {
                        invalidControls.Add(new DotvvmConfigurationAssertResult<DotvvmControlConfiguration>(control, DotvvmConfigurationAssertReason.InvalidCombination));

                    }
                }

                if (!string.IsNullOrWhiteSpace(control.Src) && loader.GetMarkup(config, control.Src) == null)
                {
                    invalidControls.Add(new DotvvmConfigurationAssertResult<DotvvmControlConfiguration>(control, DotvvmConfigurationAssertReason.MissingFile));
                }

            }

            if (invalidControls.Any() || invalidRoutes.Any())
            {
                throw new DotvvmConfigurationException(invalidRoutes, invalidControls);
            }


        }


    }
}
