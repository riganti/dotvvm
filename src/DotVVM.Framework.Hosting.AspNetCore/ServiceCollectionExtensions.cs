using System;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.AspNetCore.Runtime.Caching;
using DotVVM.Framework.Runtime.Caching;
using DotVVM.Framework.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Builder;
using System.Reflection;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting.AspNetCore;
using DotVVM.Framework.Diagnostics;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Hosting.AspNetCore.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static readonly DiagnosticsStartupTracer startupTracer = new DiagnosticsStartupTracer();

        /// <summary>
        /// Adds DotVVM services with authorization and data protection to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        // ReSharper disable once InconsistentNaming
        public static IServiceCollection AddDotVVM<TServiceConfigurator>(this IServiceCollection services) where TServiceConfigurator : IDotvvmServiceConfigurator, new()
        {
            AddDotVVMServices(services);

            var configurator = new TServiceConfigurator();
            var dotvvmServices = new DotvvmServiceCollection(services);

            startupTracer.TraceEvent(StartupTracingConstants.DotvvmConfigurationUserServicesRegistrationStarted);
            configurator.ConfigureServices(dotvvmServices);
            startupTracer.TraceEvent(StartupTracingConstants.DotvvmConfigurationUserServicesRegistrationFinished);

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

        // ReSharper disable once InconsistentNaming
        private static void AddDotVVMServices(IServiceCollection services)
        {
            startupTracer.TraceEvent(StartupTracingConstants.AddDotvvmStarted);

            var addAuthorizationMethod =
                Type.GetType("Microsoft.Extensions.DependencyInjection.AuthorizationServiceCollectionExtensions, Microsoft.AspNetCore.Authorization", throwOnError: false)
                    ?.GetMethod("AddAuthorization", new[] { typeof(IServiceCollection) })
                ?? Type.GetType("Microsoft.Extensions.DependencyInjection.PolicyServiceCollectionExtensions, Microsoft.AspNetCore.Authorization.Policy", throwOnError: false)
                    ?.GetMethod("AddAuthorization", new[] { typeof(IServiceCollection) })
                ?? throw new InvalidOperationException("Unable to find ASP.NET Core AddAuthorization method. You are probably using an incompatible version of ASP.NET Core.");
            addAuthorizationMethod.Invoke(null, new object[] { services });

            services.AddDataProtection();
            services.AddMemoryCache();
            DotvvmServiceCollectionExtensions.RegisterDotVVMServices(services);

            services.TryAddSingleton<ICsrfProtector, DefaultCsrfProtector>();
            services.TryAddSingleton<ICookieManager, ChunkingCookieManager>();
            services.TryAddSingleton<IDotvvmCacheAdapter, DefaultDotvvmCacheAdapter>();
            services.TryAddSingleton<IViewModelProtector, DefaultViewModelProtector>();
            services.TryAddSingleton<IEnvironmentNameProvider, DotvvmEnvironmentNameProvider>();
            services.TryAddSingleton<IRequestCancellationTokenProvider, RequestCancellationTokenProvider>();
            services.TryAddScoped<DotvvmRequestContextStorage>(_ => new DotvvmRequestContextStorage());
            services.TryAddScoped<IDotvvmRequestContext>(s => s.GetRequiredService<DotvvmRequestContextStorage>().Context);
            services.AddSingleton<IDotvvmViewCompilationService, DotvvmViewCompilationService>();
            services.AddTransient<IDotvvmWarningSink, AspNetCoreLoggerWarningSink>();

            services.TryAddSingleton<IStartupTracer>(startupTracer);
        }
    }
}
