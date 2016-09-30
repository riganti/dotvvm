using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds MVC services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        public static IDotvvmBuilder AddDotVVM(this IServiceCollection services)
        {
            services
                .AddAuthorization()
                .AddDataProtection();

            services.AddDotVVMCore();

            services.TryAddSingleton<ICsrfProtector, DefaultCsrfProtector>();
            services.TryAddSingleton<IViewModelProtector, DefaultViewModelProtector>();

            return new DotvvmBuilder(services);
        }
    }
}