using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using DotVVM.Framework.Diagnostics;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds DotVVM to the <see cref="IApplicationBuilder" /> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" /> instance.</param>
        /// <param name="applicationRootPath">
        /// The path to application's root directory. It is used to resolve paths to views, etc.
        /// The default value is equal to <see cref="IHostingEnvironment.ContentRootPath" />.
        /// </param>
        /// <param name="useErrorPages">
        /// A value indicating whether to show detailed error page if an exception occurs. It is enabled by default
        /// if <see cref="HostingEnvironmentExtensions.IsDevelopment" /> returns <c>true</c>.
        /// </param>
        /// <param name="modifyConfiguration">An action that allows modifying configuration before it's frozen.</param>
        public static DotvvmConfiguration UseDotVVM<TStartup>(this IApplicationBuilder app, string applicationRootPath = null, bool? useErrorPages = null, Action<DotvvmConfiguration> modifyConfiguration = null)
            where TStartup : IDotvvmStartup, new()
        {
            return app.UseDotVVM(new TStartup(), applicationRootPath, useErrorPages, modifyConfiguration);
        }

        /// <summary>
        /// Adds DotVVM to the <see cref="IApplicationBuilder" /> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" /> instance.</param>
        /// <param name="startup">The <see cref="IDotvvmStartup" /> instance.</param>
        /// <param name="applicationRootPath">
        /// The path to application's root directory. It is used to resolve paths to views, etc.
        /// The default value is equal to <see cref="IHostingEnvironment.ContentRootPath" />.
        /// </param>
        /// <param name="useErrorPages">
        /// A value indicating whether to show detailed error page if an exception occurs. It is enabled by default
        /// if <see cref="HostingEnvironmentExtensions.IsDevelopment" /> returns <c>true</c>.
        /// </param>
        /// <param name="modifyConfiguration">An action that allows modifying configuration before it's frozen.</param>
        public static DotvvmConfiguration UseDotVVM(this IApplicationBuilder app, IDotvvmStartup startup, string applicationRootPath, bool? useErrorPages, Action<DotvvmConfiguration> modifyConfiguration = null)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var config = app.ApplicationServices.GetRequiredService<DotvvmConfiguration>();
            config.Debug = env.IsDevelopment();
            config.ApplicationPhysicalPath = applicationRootPath ?? env.ContentRootPath;

            var startupTracer = app.ApplicationServices.GetRequiredService<IStartupTracer>();
            startupTracer.TraceEvent(StartupTracingConstants.DotvvmConfigurationUserConfigureStarted);
            startup.Configure(config, applicationRootPath);
            startupTracer.TraceEvent(StartupTracingConstants.DotvvmConfigurationUserConfigureFinished);

            modifyConfiguration?.Invoke(config);
            config.Freeze();

            startupTracer.TraceEvent(StartupTracingConstants.UseDotvvmStarted);
            app.UseMiddleware<DotvvmMiddleware>(config, new List<IMiddleware> {
                ActivatorUtilities.CreateInstance<DotvvmCsrfTokenMiddleware>(config.ServiceProvider),
                ActivatorUtilities.CreateInstance<DotvvmLocalResourceMiddleware>(app.ApplicationServices),
                DotvvmFileUploadMiddleware.TryCreate(app.ApplicationServices),
                new DotvvmReturnedFileMiddleware(),
                new DotvvmRoutingMiddleware()
            }.Where(t => t != null).ToArray(), useErrorPages ?? config.Debug);

            startupTracer.TraceEvent(StartupTracingConstants.UseDotvvmFinished);

            var compilationConfiguration = config.Markup.ViewCompilation;
            compilationConfiguration.HandleViewCompilation(config, startupTracer);

            if (config.ServiceProvider.GetService<IDiagnosticsInformationSender>() is IDiagnosticsInformationSender sender)
            {
                startupTracer.NotifyStartupCompleted(sender);
            }

            return config;
        }
    }
}
