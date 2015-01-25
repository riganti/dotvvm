using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Owin;
using Redwood.Framework.Configuration;
using Redwood.Framework.Hosting;

namespace Redwood.Framework
{
    public static class OwinExtensions
    {

        public static RedwoodConfiguration UseRedwood(this IAppBuilder app, string applicationRootDirectory)
        {
            var configurationFilePath = Path.Combine(applicationRootDirectory, "redwood.json");

            // load or create default configuration
            var configuration = RedwoodConfiguration.CreateDefault();
            if (File.Exists(configurationFilePath))
            {
                var fileContents = File.ReadAllText(configurationFilePath);
                JsonConvert.PopulateObject(fileContents, configuration);
            }
            configuration.ApplicationPhysicalPath = applicationRootDirectory;
            configuration.Markup.AddAssembly(Assembly.GetCallingAssembly().FullName);

            // add middlewares
            app.Use<RedwoodErrorPageMiddleware>();

            app.Use<RedwoodRestrictedStaticFilesMiddleware>();
            app.Use<RedwoodEmbeddedResourceMiddleware>();

            app.Use<RedwoodMiddleware>(configuration);
            
            return configuration;
        }
        
    }
}
