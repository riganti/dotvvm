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
        public static IServiceCollection AddDotVVM<TServicesConfiguration>(this IServiceCollection services) where TServicesConfiguration : IDotvvmServiceConfigurator, new()
        {
            AddDotVVMServices(services);

            var options = new TServicesConfiguration();
            var builder = new DotvvmOptions(services);
            options.ConfigureServices(builder);

            return services;
        }



        /// <summary>
        /// Adds DotVVM services with authorization and data protection to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        // ReSharper disable once InconsistentNaming
        public static IServiceCollection AddDotVVM(this IServiceCollection services)
        {
            AddDotVVMServices(services);
            return services;
        }
        private static void AddDotVVMServices(IServiceCollection services)
        {
            services
                .AddAuthorization()
                .AddDataProtection();

            DotvvmServiceCollectionExtensions.RegisterDotVVMServices(services);

            services.TryAddSingleton<ICsrfProtector, DefaultCsrfProtector>();
            services.TryAddSingleton<ICookieManager, ChunkingCookieManager>();
            services.TryAddSingleton<IViewModelProtector, DefaultViewModelProtector>();
            services.TryAddSingleton<IEnvironmentNameProvider, DotvvmEnvironmentNameProvider>();
            services.TryAddScoped<DotvvmRequestContextStorage>(_ => new DotvvmRequestContextStorage());
            services.TryAddScoped<IDotvvmRequestContext>(s => s.GetRequiredService<DotvvmRequestContextStorage>().Context);
        }
    }
}
