using System.IO;
using System.Reflection;
using DotVVM.Framework.Configuration;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Newtonsoft.Json;
using Owin;

namespace DotVVM.Framework.Hosting
{
    public static class OwinExtensions
    {
        public static DotvvmConfiguration CreateConfiguration(string applicationRootDirectory)
        {
            // load or create default configuration
            var configuration = DotvvmConfiguration.CreateDefault();
            configuration.ApplicationPhysicalPath = applicationRootDirectory;
            configuration.Markup.AddAssembly(Assembly.GetCallingAssembly().FullName);
            return configuration;
        }

        internal static DotvvmConfiguration UseDotVVM(this IAppBuilder app, string applicationRootDirectory, bool errorPages = true)
        {
            var configuration = CreateConfiguration(applicationRootDirectory);
            configuration.ServiceLocator.RegisterSingleton<IDataProtectionProvider>(app.GetDataProtectionProvider);

            // add middlewares
            if (errorPages)
                app.Use<DotvvmErrorPageMiddleware>();

            app.Use<DotvvmRestrictedStaticFilesMiddleware>();
            app.Use<DotvvmEmbeddedResourceMiddleware>();
            app.Use<DotvvmFileUploadMiddleware>(configuration);
            app.Use<JQueryGlobalizeCultureMiddleware>();

            app.Use<DotvvmReturnedFileMiddleware>(configuration);

            app.Use<DotvvmMiddleware>(configuration);

            return configuration;
        }

        public static DotvvmConfiguration UseDotVVM<TStartup>(this IAppBuilder app, string applicationRootDirectory, bool errorPages = true)
            where TStartup: IDotvvmStartup, new()
        {
            var config = app.UseDotVVM(applicationRootDirectory, errorPages);
            new TStartup().Configure(config, applicationRootDirectory);
            return config;
        }

        public static DotvvmRequestContext GetDotvvmContext(this IOwinContext owinContext)
        {
            return DotvvmRequestContext.GetCurrent(owinContext);
        }
    }
}
