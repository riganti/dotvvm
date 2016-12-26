using System;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds DotVVM services with authorization and data protection to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="options">A method that can set additional DotVVM options, like temporary file stores, or replace default DotVVM components with custom ones.</param>
        public static IServiceCollection AddDotVVM(this IServiceCollection services, Action<IDotvvmOptions> options = null)
        {
            services
                .AddAuthorization()
                .AddDataProtection();

            DotvvmServiceCollectionExtensions.RegisterDotVVMServices(services);

            services.TryAddSingleton<ICsrfProtector, DefaultCsrfProtector>();
            services.TryAddSingleton<IViewModelProtector, DefaultViewModelProtector>();
            services.TryAddSingleton<IEnvironmentNameProvider, DotvvmEnvironmentNameProvider>();

            if (options != null)
            {
                var builder = new DotvvmOptions(services);
                options(builder);
            }

            return services;
        }
    }
}