using System;
using System.Collections.Generic;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        /// <param name="builder">An action to configure DotVVM services and extensions.</param>
        public static DotvvmConfiguration UseDotVVM<TStartup>(this IAppBuilder app, string applicationRootPath, bool useErrorPages = true, bool debug = true, Action<IDotvvmBuilder> builder = null)
            where TStartup : IDotvvmStartup, new()
        {
            var config = app.UseDotVVM(applicationRootPath, useErrorPages, debug, builder);
            new TStartup().Configure(config, applicationRootPath);
            return config;
        }

        private static DotvvmConfiguration UseDotVVM(this IAppBuilder app, string applicationRootPath, bool useErrorPages, bool debug, Action<IDotvvmBuilder> builder)
        {
            var config = DotvvmConfiguration.CreateDefault(s =>
            {
                s.TryAddSingleton<IDataProtectionProvider>(p => new DefaultDataProtectionProvider(app));
                s.TryAddSingleton<IViewModelProtector, DefaultViewModelProtector>();
                s.TryAddSingleton<ICsrfProtector, DefaultCsrfProtector>();
                s.TryAddSingleton<IEnvironmentNameProvider, DotvvmEnvironmentNameProvider>();
                builder?.Invoke(new DotvvmBuilder(s));
            });

            config.Debug = debug;
            config.ApplicationPhysicalPath = applicationRootPath;

            if (useErrorPages)
            {
                app.Use<DotvvmErrorPageMiddleware>();
            }

            app.Use<DotvvmMiddleware>(config, new List<IMiddleware> {
                ActivatorUtilities.CreateInstance<DotvvmLocalResourceMiddleware>(config.ServiceLocator.GetServiceProvider()),
                new DotvvmFileUploadMiddleware(),
                new DotvvmReturnedFileMiddleware(),
                new DotvvmRoutingMiddleware()
            });

            return config;
        }
    }
}