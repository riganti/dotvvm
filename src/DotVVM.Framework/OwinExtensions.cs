using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Owin;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework
{
    public static class OwinExtensions
    {
        public static DotvvmConfiguration CreateConfiguration(string applicationRootDirectory)
        {
            var configurationFilePath = Path.Combine(applicationRootDirectory, "dotvvm.json");

            // load or create default configuration
            var configuration = DotvvmConfiguration.CreateDefault();
            if (File.Exists(configurationFilePath))
            {
                var fileContents = File.ReadAllText(configurationFilePath);
                JsonConvert.PopulateObject(fileContents, configuration);
            }
            configuration.ApplicationPhysicalPath = applicationRootDirectory;
            configuration.Markup.AddAssembly(Assembly.GetCallingAssembly().FullName);
            return configuration;
        }

        public static DotvvmConfiguration UseDotVVM(this IAppBuilder app, string applicationRootDirectory, bool errorPages = true)
        {
            var configuration = CreateConfiguration(applicationRootDirectory);

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
    }
}
