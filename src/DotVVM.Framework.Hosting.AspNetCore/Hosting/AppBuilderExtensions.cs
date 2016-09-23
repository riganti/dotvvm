using System;
using System.Collections.Generic;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Hosting
{
    public static class AppBuilderExtensions
    {
        public static void AddDotvvmServices(this IServiceCollection collection)
        {
            collection.AddAuthorization();
            ServiceConfigurationHelper.AddDotvvmCoreServices(collection);
            collection.AddSingleton<ICsrfProtector, DefaultCsrfProtector>();
            collection.AddSingleton<IViewModelProtector, DefaultViewModelProtector>();
        }

        public static DotvvmConfiguration CreateConfiguration(string applicationRootDirectory, IServiceProvider serviceProvider)
        {
            // load or create default configuration
            var configuration = serviceProvider.GetService<DotvvmConfiguration>();

            if (configuration == null)
            {
                throw new InvalidOperationException("Service provider does not contain DotvvmConfiguration service. Make sure you have Dotvvm services registered in ConfigureServices method of your Startup class.");
            }

            configuration.Runtime.GlobalFilters.Add(new AuthorizeFilterAttribute());
            configuration.ApplicationPhysicalPath = applicationRootDirectory;
            return configuration;
        }

        internal static DotvvmConfiguration UseDotVVM(this IApplicationBuilder app, string applicationRootDirectory, bool errorPages = true)
        {
            var configuration = CreateConfiguration(applicationRootDirectory, app.ApplicationServices);
#if Owin
            configuration.ServiceLocator.RegisterSingleton<IDataProtectionProvider>(app.GetDataProtectionProvider);
#endif

            var middlewares = new List<IMiddleware>();

            // add middlewares
            if (errorPages)
            {
                app.UseMiddleware<DotvvmErrorPageMiddleware>();
            }

            middlewares.Add(new DotvvmEmbeddedResourceMiddleware());
            middlewares.Add(new DotvvmFileUploadMiddleware());
            middlewares.Add(new JQueryGlobalizeCultureMiddleware());
            middlewares.Add(new DotvvmReturnedFileMiddleware());
            middlewares.Add(new DotvvmRoutingMiddleware());

            app.UseMiddleware<DotvvmMiddleware>(configuration, middlewares);

            return configuration;
        }

        public static DotvvmConfiguration UseDotVVM<TStartup>(this IApplicationBuilder app, string applicationRootDirectory, bool errorPages = true)
            where TStartup : IDotvvmStartup, new()
        {
            var config = app.UseDotVVM(applicationRootDirectory, errorPages);
            new TStartup().Configure(config, applicationRootDirectory);
            return config;
        }
    }
}