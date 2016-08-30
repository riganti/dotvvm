using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using DotVVM.Framework.Configuration;
using Newtonsoft.Json;
using System;
using DotVVM.Framework.Hosting.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Owin;
using DotVVM.Framework.Security;
using Microsoft.Owin.Security.DataProtection;

namespace DotVVM.Framework.Hosting
{
    public static class AppBuilderExtensions
    {
        public static DotvvmConfiguration CreateConfiguration(string applicationRootDirectory, Action<IServiceCollection> registerServices = null)
        {
            // load or create default configuration
            var configuration = DotvvmConfiguration.CreateDefault(registerServices);
            configuration.ApplicationPhysicalPath = applicationRootDirectory;
            return configuration;
        }

        internal static DotvvmConfiguration UseDotVVM(this IAppBuilder app, string applicationRootDirectory, bool errorPages = true, Action<IServiceCollection> registerServices = null)
        {
            var configuration = CreateConfiguration(applicationRootDirectory, c =>
            {
                c.AddSingleton<IViewModelProtector, DefaultViewModelProtector>();
                c.AddSingleton<ICsrfProtector, DefaultCsrfProtector>();
                c.AddSingleton<IDataProtectionProvider>(s => new DefaultDataProtectionProvider(app));
                registerServices(c);
            });
#if Owin
            configuration.ServiceLocator.RegisterSingleton<IDataProtectionProvider>(app.GetDataProtectionProvider);
#endif
            
            // add middlewares
            if (errorPages)
                app.Use<DotvvmErrorPageMiddleware>();
            
            app.Use<DotvvmEmbeddedResourceMiddleware>();
            app.Use<DotvvmFileUploadMiddleware>(configuration);
            app.Use<JQueryGlobalizeCultureMiddleware>();

            app.Use<DotvvmReturnedFileMiddleware>(configuration);

            app.Use<DotvvmMiddleware>(configuration);

            return configuration;
        }

        public static DotvvmConfiguration UseDotVVM<TStartup>(this IAppBuilder app, string applicationRootDirectory, bool errorPages = true, Action<IServiceCollection> registerServices = null)
            where TStartup: IDotvvmStartup, new()
        {
            var config = app.UseDotVVM(applicationRootDirectory, errorPages, registerServices);
            new TStartup().Configure(config, applicationRootDirectory);
            return config;
        }
    }
}
