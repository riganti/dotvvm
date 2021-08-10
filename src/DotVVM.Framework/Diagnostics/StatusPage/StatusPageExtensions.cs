using System;
using System.Linq;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Diagnostics.StatusPage
{
    public static class StatusPageExtensions
    {
        /// <summary>
        /// Adds Compilation Status Page to the application.
        /// </summary>
        public static IDotvvmServiceCollection AddStatusPage(this IDotvvmServiceCollection services, Action<StatusPageOptions> configure)
        {
            var options = StatusPageOptions.CreateDefaultOptions();
            configure?.Invoke(options);

            return services.AddStatusPage(options);
        }

        /// <summary>
        /// Adds Compilation Status Page to the application.
        /// </summary>
        public static IDotvvmServiceCollection AddStatusPage(this IDotvvmServiceCollection services, StatusPageOptions options = null)
        {
            if (options == null) options = StatusPageOptions.CreateDefaultOptions();

            if (services.Services.All(t => t.ServiceType != typeof(IDotvvmViewCompilationService)))
                services.Services.AddSingleton<IDotvvmViewCompilationService, DotvvmViewCompilationService>();

            services.Services.AddSingleton<StatusPageOptions>(options);
            services.Services.AddTransient<StatusPagePresenter>();

            services.Services.Configure((DotvvmConfiguration config) =>
            {
                config.RouteTable.Add(options.RouteName, options.Url, "embedded://DotVVM.Diagnostics.StatusPage/StatusPage/Status.dothtml", null, s => s.GetService<StatusPagePresenter>());
            });

            return services;
        }

        /// <summary>
        /// Adds Compilation Status Page to the application.
        /// </summary>
        public static IDotvvmServiceCollection AddStatusPageApi(this IDotvvmServiceCollection services, Action<StatusPageApiOptions> configure)
        {
            var options = StatusPageApiOptions.CreateDefaultOptions();
            configure?.Invoke(options);

            return services.AddStatusPageApi(options);
        }
        /// <summary>
        /// Adds Compilation Status Page to the application.
        /// </summary>
        public static IDotvvmServiceCollection AddStatusPageApi(this IDotvvmServiceCollection services, StatusPageApiOptions options = null)
        {
            if (options == null) options = StatusPageApiOptions.CreateDefaultOptions();

            if (services.Services.All(t=>t.ServiceType != typeof(IDotvvmViewCompilationService)))
                services.Services.AddSingleton<IDotvvmViewCompilationService, DotvvmViewCompilationService>();

            services.Services.AddSingleton<StatusPageApiOptions>(options);
            services.Services.AddTransient<StatusPageApiPresenter>();

            services.Services.Configure((DotvvmConfiguration config) =>
            {
                config.RouteTable.Add(options.RouteName, options.Url,typeof(StatusPageApiPresenter));
            });

            return services;
        }
    }
}
