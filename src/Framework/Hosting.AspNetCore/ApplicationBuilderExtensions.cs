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
using DotVVM.Framework.Compilation.ControlTree;

#if NET5_0_OR_GREATER
using HostingEnv = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
using Microsoft.Extensions.Hosting;
#else
using HostingEnv = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
#endif

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
#pragma warning disable CS1574 // the referenced HostingEnv.WebRootPath does not exist on netstandard2.1 and there is no way to do ifdefs in comments...
        /// <summary>
        /// Adds DotVVM to the <see cref="IApplicationBuilder" /> request execution pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" /> instance.</param>
        /// <param name="applicationRootPath">
        /// The path to application's root directory. It is used to resolve paths to views, etc.
        /// The default value is equal to <see cref="HostingEnv.WebRootPath" />.
        /// </param>
        /// <param name="useErrorPages">
        /// A value indicating whether to show detailed error page if an exception occurs. It is enabled by default
        /// if <see cref="HostEnvironmentEnvExtensions.IsDevelopment" /> returns <c>true</c>.
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
        /// The default value is equal to <see cref="HostingEnv.WebRootPath" />.
        /// </param>
        /// <param name="useErrorPages">
        /// A value indicating whether to show detailed error page if an exception occurs. It is enabled by default
        /// if <see cref="HostEnvironmentEnvExtensions.IsDevelopment" /> returns <c>true</c>.
        /// </param>
        /// <param name="modifyConfiguration">An action that allows modifying configuration before it's frozen.</param>
        public static DotvvmConfiguration UseDotVVM(this IApplicationBuilder app, IDotvvmStartup startup, string applicationRootPath, bool? useErrorPages, Action<DotvvmConfiguration> modifyConfiguration = null)
        {
            var env = app.ApplicationServices.GetRequiredService<HostingEnv>();
            var tokenMiddleware = Task.Run(() => ActivatorUtilities.CreateInstance<DotvvmCsrfTokenMiddleware>(app.ApplicationServices));
            var config = app.ApplicationServices.GetRequiredService<DotvvmConfiguration>();
            // warm up the translator
            _ = Task.Run(() => config.Markup.JavascriptTranslator);
            config.Debug = env.IsDevelopment();
            config.ApplicationPhysicalPath = applicationRootPath ?? env.ContentRootPath;

            var startupTracer = app.ApplicationServices.GetRequiredService<IStartupTracer>();
            startupTracer.TraceEvent(StartupTracingConstants.DotvvmConfigurationUserConfigureStarted);
            config.Markup.AddAssembly(startup.GetType().Assembly);
            startup.Configure(config, applicationRootPath);
            startupTracer.TraceEvent(StartupTracingConstants.DotvvmConfigurationUserConfigureFinished);

            modifyConfiguration?.Invoke(config);
            config.Diagnostics.Apply(config);
            config.Freeze();
            // warm up the resolver in the background
            Task.Run(() => app.ApplicationServices.GetService(typeof(IControlResolver)));
            Task.Run(() => VisualStudioHelper.DumpConfiguration(config, config.ApplicationPhysicalPath));

            startupTracer.TraceEvent(StartupTracingConstants.UseDotvvmStarted);
            app.UseMiddleware<DotvvmMiddleware>(config, new List<IMiddleware> {
                tokenMiddleware.Result,
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
