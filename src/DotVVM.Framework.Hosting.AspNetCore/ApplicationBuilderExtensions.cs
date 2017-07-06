using System.Collections.Generic;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

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
            var config = app.UseDotVVM(applicationRootPath, useErrorPages);
            new TStartup().Configure(config, applicationRootPath);
            return config;
        }

        internal static DotvvmConfiguration UseDotVVM(this IApplicationBuilder app, string applicationRootPath, bool? useErrorPages)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var config = app.ApplicationServices.GetRequiredService<DotvvmConfiguration>();

            config.Debug = env.IsDevelopment();
            config.ApplicationPhysicalPath = applicationRootPath ?? env.ContentRootPath;

            config.Runtime.Reporters.AddRange(config.ServiceLocator.GetServiceProvider().GetServices<IRequestTracingReporter>());

            if (useErrorPages ?? env.IsDevelopment())
            {
                app.UseMiddleware<DotvvmErrorPageMiddleware>();
            }

            app.UseMiddleware<DotvvmMiddleware>(config, new List<IMiddleware> {
                ActivatorUtilities.CreateInstance<DotvvmLocalResourceMiddleware>(app.ApplicationServices),
                new DotvvmFileUploadMiddleware(),
                new DotvvmReturnedFileMiddleware(),
                new DotvvmRoutingMiddleware()
            });

            return config;
        }
    }
}