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
            RedwoodConfiguration configuration;
            var configurationFilePath = Path.Combine(applicationRootDirectory, "redwood.json");

            // load or create default configuration
            if (File.Exists(configurationFilePath))
            {
                var fileContents = File.ReadAllText(configurationFilePath);
                configuration = JsonConvert.DeserializeObject<RedwoodConfiguration>(fileContents);
            }
            else
            {
                configuration = RedwoodConfiguration.CreateDefault();
            }
            configuration.ApplicationPhysicalPath = applicationRootDirectory;
            configuration.Markup.Assemblies.Add(Assembly.GetCallingAssembly().FullName);

            // add middleware
            app.Use<RedwoodMiddleware>(configuration);

            return configuration;
        }

    }
}
