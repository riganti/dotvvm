using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using DotVVM.Framework.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Builder;
#if DotNetCore
using AppBuilder = Microsoft.AspNetCore.Builder.IApplicationBuilder;
#else
using AppBuilder = Owin.IApplicationBuilder;
#endif

namespace DotVVM.Framework.Hosting
{
    public static class AppBuilderExtensions
    {
        public static DotvvmConfiguration CreateConfiguration(string applicationRootDirectory)
        {
            // load or create default configuration
            var configuration = DotvvmConfiguration.CreateDefault();
            configuration.ApplicationPhysicalPath = applicationRootDirectory;
            return configuration;
        }

        internal static DotvvmConfiguration UseDotVVM(this AppBuilder app, string applicationRootDirectory, bool errorPages = true)
        {
            var configuration = CreateConfiguration(applicationRootDirectory);
#if !DotNetCore
            configuration.ServiceLocator.RegisterSingleton<IDataProtectionProvider>(app.GetDataProtectionProvider);
#endif
            
            // add middlewares
            if (errorPages)
                app.UseMiddleware<DotvvmErrorPageMiddleware>();
            
            app.UseMiddleware<DotvvmEmbeddedResourceMiddleware>();
            app.UseMiddleware<DotvvmFileUploadMiddleware>(configuration);
            app.UseMiddleware<JQueryGlobalizeCultureMiddleware>();

            app.UseMiddleware<DotvvmReturnedFileMiddleware>(configuration);

            app.UseMiddleware<DotvvmMiddleware>(configuration);

            return configuration;
        }

        public static DotvvmConfiguration UseDotVVM<TStartup>(this AppBuilder app, string applicationRootDirectory, bool errorPages = true)
            where TStartup: IDotvvmStartup, new()
        {
            var config = app.UseDotVVM(applicationRootDirectory, errorPages);
            new TStartup().Configure(config, applicationRootDirectory);
            return config;
        }
    }
}
