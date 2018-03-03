using System;
using System.Linq;
using System.Collections.Generic;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        /// <param name="debug">A value indicating whether the application should run in debug mode.</param>
        public static DotvvmConfiguration UseDotVVM<TStartup>(this IApplicationBuilder app, string applicationRootPath = null, bool? useErrorPages = null, bool? debug = null)
            where TStartup : IDotvvmStartup, new()
        {
            var config = app.UseDotVVM(applicationRootPath, useErrorPages, debug);
            new TStartup().Configure(config, applicationRootPath);
            return config;
        }

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
        /// <param name="debug">A value indicating whether the application should run in debug mode.</param>
        public static DotvvmConfiguration UseDotVVM(this IApplicationBuilder app, string applicationRootPath, bool? useErrorPages, bool? debug)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var config = app.ApplicationServices.GetRequiredService<DotvvmConfiguration>();

            new EnvironmentConfigurationInitializer().Initialize(config, debug ?? env.IsDevelopment());

            config.ApplicationPhysicalPath = applicationRootPath ?? env.ContentRootPath;

            if (useErrorPages ?? env.IsDevelopment())
            {
                app.UseMiddleware<DotvvmErrorPageMiddleware>();
            }

            app.UseMiddleware<DotvvmMiddleware>(config, new List<IMiddleware> {
                ActivatorUtilities.CreateInstance<DotvvmLocalResourceMiddleware>(app.ApplicationServices),
                DotvvmFileUploadMiddleware.TryCreate(app.ApplicationServices),
                new DotvvmReturnedFileMiddleware(),
                new DotvvmRoutingMiddleware()
            }.Where(t => t != null).ToArray());

            return config;
        }
    }
}
