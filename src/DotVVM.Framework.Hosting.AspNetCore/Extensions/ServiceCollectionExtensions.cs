using System;
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
        public static IDotvvmBuilder AddDotVVM(this IServiceCollection services)
        {
            services
                .AddAuthorization()
                .AddDataProtection();

            DotvvmServiceCollectionExtensions.RegisterDotVVMServices(services);

            services.TryAddSingleton<ICsrfProtector, DefaultCsrfProtector>();
            services.TryAddSingleton<IViewModelProtector, DefaultViewModelProtector>();

            return new DotvvmBuilder(services);
        }

        /// <summary>
        /// Adds DotVVM services with authorization and data protection to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="dotvvmBuilderAction">Method which executes additional configuration actions of DotVVM services.</param>
        public static IServiceCollection AddDotVVM(this IServiceCollection services, Action<IDotvvmBuilder> dotvvmBuilderAction)
        {
            var builder = services.AddDotVVM();
            dotvvmBuilderAction(builder);
            return services;
        }
    }
}