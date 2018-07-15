using DotVVM.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.ResourceManagement;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Diagnostics.StatusPage
{
    public static class StatusPageExtensions
    {
        public static IDotvvmServiceCollection AddStatusPageConfiguration(this IDotvvmServiceCollection services)
        {
            return services.AddStatusPageConfiguration(StatusPageOptions.CreateDefaultOptions());
        }

        /// <summary>
        /// Adds Compilation Status Page to the application.
        /// </summary>
        public static IDotvvmServiceCollection AddStatusPageConfiguration(this IDotvvmServiceCollection services, StatusPageOptions options)
        {
            if (options == null)
            {
                options = StatusPageOptions.CreateDefaultOptions();
            }

            services.Services.Configure((DotvvmConfiguration config) =>
            {
                config.RouteTable.Add(options.RouteName, options.Url, "embedded://DotVVM.Diagnostics.StatusPage/Status.dothtml");
            });

            return services;
        }
    }
}
