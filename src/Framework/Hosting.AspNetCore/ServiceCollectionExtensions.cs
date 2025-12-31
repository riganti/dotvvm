using System;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Caching;
using DotVVM.Framework.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Builder;
using System.Reflection;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting.AspNetCore;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Hosting.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting;
#if NET9_0_OR_GREATER
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting.AspNetCore.StaticAssets;
#endif

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds DotVVM services with authorization and data protection to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        // ReSharper disable once InconsistentNaming
        public static IServiceCollection AddDotVVM<TServiceConfigurator>(this IServiceCollection services, IStartupTracer startupTracer = null)
            where TServiceConfigurator : IDotvvmServiceConfigurator, new()
        {
            var configurator = new TServiceConfigurator();
            return services.AddDotVVM(configurator, startupTracer);
        }

        /// <summary>
        /// Adds DotVVM services with authorization and data protection to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="configurator">The <see cref="IDotvvmServiceConfigurator"/> instance.</param>
        public static IServiceCollection AddDotVVM(this IServiceCollection services, IDotvvmServiceConfigurator configurator, IStartupTracer startupTracer = null)
        {
            startupTracer ??= new NullStartupTracer();
            AddDotVVMServices(services, startupTracer);

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
        public static IServiceCollection AddDotVVM(this IServiceCollection services, IStartupTracer startupTracer = null)
        {
            AddDotVVMServices(services, startupTracer);
            return services;
        }

        // ReSharper disable once InconsistentNaming
        private static void AddDotVVMServices(IServiceCollection services, IStartupTracer startupTracer)
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
#if NET9_0_OR_GREATER
            services.TryAddSingleton<StaticAssetsProvider>();
            services.ConfigureWithServices<DotvvmConfiguration>(static (config, services) =>
            {
                var provider = services.GetRequiredService<StaticAssetsProvider>();
                var env = services.GetRequiredService<IWebHostEnvironment>();
                
                config.Resources.RegisterNamedParent("asset", new StaticAssetResourceRepository(config, provider, env));
                
                // Replace default virtual path transformers with static asset transformer
                var lazyTransformer = new Lazy<IHtmlAttributeTransformer>(() => new StaticAssetHtmlAttributeTransformer(provider));
                var staticAssetTransform = new HtmlAttributeTransformConfiguration();
                staticAssetTransform.SetInstance(lazyTransformer, typeof(StaticAssetHtmlAttributeTransformer));
                config.Markup.HtmlAttributeTransforms[new("a", "href")] = staticAssetTransform;
                config.Markup.HtmlAttributeTransforms[new("link", "href")] = staticAssetTransform;
                config.Markup.HtmlAttributeTransforms[new("img", "src")] = staticAssetTransform;
                config.Markup.HtmlAttributeTransforms[new("iframe", "src")] = staticAssetTransform;
                config.Markup.HtmlAttributeTransforms[new("script", "src")] = staticAssetTransform;
                config.Markup.HtmlAttributeTransforms[new("meta", "content")] = staticAssetTransform;
            });
#endif
            DotvvmServiceCollectionExtensions.RegisterDotVVMServices(services);

            services.TryAddSingleton<ICsrfProtector, DefaultCsrfProtector>();
            services.TryAddSingleton<ICookieManager, ChunkingCookieManager>();
            services.TryAddSingleton<IViewModelProtector, DefaultViewModelProtector>();
            services.TryAddSingleton<IEnvironmentNameProvider, DotvvmEnvironmentNameProvider>();

            services.TryAddSingleton<IStartupTracer>(startupTracer);

            DotvvmHealthCheck.RegisterHealthCheck(services);
        }
    }
}
