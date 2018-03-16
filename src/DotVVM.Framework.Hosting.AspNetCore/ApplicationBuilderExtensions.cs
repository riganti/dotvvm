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
        public static DotvvmConfiguration UseDotVVM<TStartup>(this IApplicationBuilder app, string applicationRootPath = null, bool? useErrorPages = null)
            where TStartup : IDotvvmStartup, new()
        {
            return app.UseDotVVM(applicationRootPath, useErrorPages, new TStartup());
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
        public static DotvvmConfiguration UseDotVVM(this IApplicationBuilder app, string applicationRootPath, bool? useErrorPages)
        {
            return UseDotVVM(app, applicationRootPath, useErrorPages, null);
        }

        private static DotvvmConfiguration UseDotVVM(this IApplicationBuilder app, string applicationRootPath, bool? useErrorPages, IDotvvmStartup startup)
        {

            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var config = app.ApplicationServices.GetRequiredService<DotvvmConfiguration>();
            config.Debug = env.IsDevelopment();
            config.ApplicationPhysicalPath = applicationRootPath ?? env.ContentRootPath;
            startup.Configure(config, applicationRootPath);

            if (useErrorPages ?? config.Debug)
            {
                app.UseMiddleware<DotvvmErrorPageMiddleware>();
            }

            app.UseMiddleware<DotvvmMiddleware>(config, new List<IMiddleware>
            {
                ActivatorUtilities.CreateInstance<DotvvmLocalResourceMiddleware>(app.ApplicationServices),
                DotvvmFileUploadMiddleware.TryCreate(app.ApplicationServices),
                new DotvvmReturnedFileMiddleware(),
                new DotvvmRoutingMiddleware()
            }.Where(t => t != null).ToArray());
            config.Freeze();
            return config;
        }
    }
}
