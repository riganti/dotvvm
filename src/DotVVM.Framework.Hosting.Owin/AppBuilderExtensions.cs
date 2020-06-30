using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Hosting.Owin.Hosting;
using DotVVM.Framework.Hosting.Owin.Runtime.Caching;
using DotVVM.Framework.Runtime.Caching;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.DataProtection;

namespace Owin
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Adds DotVVM to the <see cref="IAppBuilder" /> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder" /> instance.</param>
        /// <param name="applicationRootPath">The path to application's root directory. It is used to resolve paths to views, etc.</param>
        /// <param name="useErrorPages">
        /// A value indicating whether to show detailed error page if an exception occurs. Disable this
        /// in production.
        /// </param>
        /// <param name="debug">A value indicating whether the application should run in debug mode.</param>
        /// <param name="serviceProviderFactoryMethod">Register factory method to create your own instance of IServiceProvider.</param>
        public static DotvvmConfiguration UseDotVVM<TStartup, TServiceConfigurator>(this IAppBuilder app, string applicationRootPath, bool useErrorPages = true, bool debug = true, Func<IServiceCollection, IServiceProvider> serviceProviderFactoryMethod = null, Action<DotvvmConfiguration> modifyConfiguration = null)
            where TStartup : IDotvvmStartup, new()
            where TServiceConfigurator : IDotvvmServiceConfigurator, new()
        {
            return app.UseDotVVM(applicationRootPath, useErrorPages, debug, new TServiceConfigurator(), new TStartup(), serviceProviderFactoryMethod, modifyConfiguration);
        }

        /// <summary>
        /// Adds DotVVM to the <see cref="IAppBuilder" /> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder" /> instance.</param>
        /// <param name="applicationRootPath">The path to application's root directory. It is used to resolve paths to views, etc.</param>
        /// <param name="useErrorPages">
        /// A value indicating whether to show detailed error page if an exception occurs. Disable this
        /// in production.
        /// </param>
        /// <param name="debug">A value indicating whether the application should run in debug mode.</param>
        /// <param name="serviceProviderFactoryMethod">Register factory method to create your own instance of IServiceProvider.</param>
        public static DotvvmConfiguration UseDotVVM<TStartup>(this IAppBuilder app, string applicationRootPath, bool useErrorPages = true, bool debug = true, Func<IServiceCollection, IServiceProvider> serviceProviderFactoryMethod = null, Action<DotvvmConfiguration> modifyConfiguration = null)
            where TStartup : IDotvvmStartup, new()
        {
            var startup = new TStartup();
            return app.UseDotVVM(applicationRootPath, useErrorPages, debug, startup as IDotvvmServiceConfigurator, startup, serviceProviderFactoryMethod, modifyConfiguration);
        }

        private static DotvvmConfiguration UseDotVVM(this IAppBuilder app, string applicationRootPath, bool useErrorPages, bool debug, IDotvvmServiceConfigurator configurator, IDotvvmStartup startup, Func<IServiceCollection, IServiceProvider> serviceProviderFactoryMethod = null, Action<DotvvmConfiguration> modifyConfiguration = null)
        {
            var config = DotvvmConfiguration.CreateDefault(s => {
                s.TryAddSingleton<IDataProtectionProvider>(p => new DefaultDataProtectionProvider(app));
                s.TryAddSingleton<IDotvvmCacheAdapter, OwinDotvvmCacheAdapter>();
                s.TryAddSingleton<IViewModelProtector, DefaultViewModelProtector>();
                s.TryAddSingleton<ICsrfProtector, DefaultCsrfProtector>();
                s.TryAddSingleton<IEnvironmentNameProvider, DotvvmEnvironmentNameProvider>();
                s.TryAddSingleton<ICookieManager, ChunkingCookieManager>();
                s.TryAddSingleton<IRequestCancellationTokenProvider, RequestCancellationTokenProvider>();
                s.TryAddScoped<DotvvmRequestContextStorage>(_ => new DotvvmRequestContextStorage());
                s.TryAddScoped<IDotvvmRequestContext>(services => services.GetRequiredService<DotvvmRequestContextStorage>().Context);
                s.AddSingleton<IDotvvmViewCompilationService, DotvvmViewCompilationService>();
                configurator?.ConfigureServices(new DotvvmServiceCollection(s));
            }, serviceProviderFactoryMethod);
            config.Debug = debug;
            config.ApplicationPhysicalPath = applicationRootPath;

            startup.Configure(config, applicationRootPath);

            if (useErrorPages)
            {
                app.Use<DotvvmErrorPageMiddleware>();
            }

            modifyConfiguration?.Invoke(config);
            config.Freeze();
            
            app.Use<DotvvmMiddleware>(config, new List<IMiddleware> {
                ActivatorUtilities.CreateInstance<DotvvmCsrfTokenMiddleware>(config.ServiceProvider),
                ActivatorUtilities.CreateInstance<DotvvmLocalResourceMiddleware>(config.ServiceProvider),
                DotvvmFileUploadMiddleware.TryCreate(config.ServiceProvider),
                new DotvvmReturnedFileMiddleware(),
                new DotvvmRoutingMiddleware()
            }.Where(t => t != null).ToArray());

            var compilationConfiguration = config.Markup.ViewCompilation;
            compilationConfiguration.HandleViewCompilation(config);

            return config;
        }
    }
}
