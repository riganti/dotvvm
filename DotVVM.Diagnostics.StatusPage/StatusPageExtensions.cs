using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Diagnostics.StatusPage
{
    public static class StatusPageExtensions
    {
        /// <summary>
        /// Adds Compilation Status Page to the application.
        /// </summary>
        public static IDotvvmServiceCollection AddStatusPage(this IDotvvmServiceCollection services)
        {
            return services.AddStatusPage(StatusPageOptions.CreateDefaultOptions());
        }

        /// <summary>
        /// Adds Compilation Status Page to the application.
        /// </summary>
        public static IDotvvmServiceCollection AddStatusPage(this IDotvvmServiceCollection services, StatusPageOptions options)
        {
            if (options == null)
            {
                options = StatusPageOptions.CreateDefaultOptions();
            }

            services.Services.AddSingleton<StatusPageOptions>(options);
            services.Services.AddTransient<StatusPagePresenter>();

            services.Services.Configure((DotvvmConfiguration config) =>
            {
                config.RouteTable.Add(options.RouteName, options.Url, "embedded://DotVVM.Diagnostics.StatusPage/Status.dothtml", null, s => s.GetService<StatusPagePresenter>());
            });

            return services;
        }
    }
}
