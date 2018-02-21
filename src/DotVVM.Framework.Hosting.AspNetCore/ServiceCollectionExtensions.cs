using System;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds DotVVM services with authorization and data protection to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        public static IServiceCollection AddDotVVM<TDotvvmStartup>(this IServiceCollection services) where TDotvvmStartup : IDotvvmStartup, new()
        {
            services
                .AddAuthorization()
                .AddDataProtection();

            DotvvmServiceCollectionExtensions.RegisterDotVVMServices(services);
            services.TryAddSingleton<IDotvvmStartup>(service => new TDotvvmStartup());
            services.TryAddSingleton<ICsrfProtector, DefaultCsrfProtector>();
            services.TryAddSingleton<ICookieManager, ChunkingCookieManager>();
            services.TryAddSingleton<IViewModelProtector, DefaultViewModelProtector>();
            services.TryAddSingleton<IEnvironmentNameProvider, DotvvmEnvironmentNameProvider>();
            services.TryAddScoped<DotvvmRequestContextStorage>(_ => new DotvvmRequestContextStorage());
            services.TryAddScoped<IDotvvmRequestContext>(s => s.GetRequiredService<DotvvmRequestContextStorage>().Context);

            return services;
        }
    }
}
