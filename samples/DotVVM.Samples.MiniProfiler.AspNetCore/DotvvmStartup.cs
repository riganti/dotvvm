using DotVVM.Framework.Configuration;
using DotVVM.Tracing.MiniProfiler.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Samples.MiniProfiler.AspNetCore
{
    public class DotvvmStartup : IDotvvmStartup, IDotvvmServiceConfigurator
    {
        // For more information about this class, visit https://dotvvm.com/docs/tutorials/basics-project-structure
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            ConfigureRoutes(config, applicationPath);
            ConfigureControls(config, applicationPath);
            ConfigureResources(config, applicationPath);
        }

        private void ConfigureRoutes(DotvvmConfiguration config, string applicationPath)
        {
            config.RouteTable.Add("Default", "", "Views/default.dotmaster");
            config.RouteTable.Add("Page1", "page1", "Views/page1.dothtml");
            config.RouteTable.Add("Page2", "page2", "Views/page2.dothtml");

            // Uncomment the following line to auto-register all dothtml files in the Views folder
            // config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));    
        }

        private void ConfigureControls(DotvvmConfiguration config, string applicationPath)
        {
            // register code-only controls and markup controls
        }

        private void ConfigureResources(DotvvmConfiguration config, string applicationPath)
        {
            // register custom resources and adjust paths to the built-in resources
        }

        public void ConfigureServices(IDotvvmServiceCollection serviceCollection)
        {
            serviceCollection
                .AddDefaultTempStorages("Temp")
                .AddMiniProfilerEventTracing();

        }
    }
}
