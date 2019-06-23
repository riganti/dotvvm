using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Testing.SeleniumGenerator;
using Microsoft.Extensions.DependencyInjection;

namespace SampleApp1
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
            config.RouteTable.Add("Default", "", "Views/Default.dothtml");
            config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));    
        }

        private void ConfigureControls(DotvvmConfiguration config, string applicationPath)
        {
            // register code-only controls and markup controls
            config.Markup.AddMarkupControl("cc", "ControlA", "Controls/ControlA.dotcontrol");
            config.Markup.AddMarkupControl("cc", "ControlB", "Controls/ControlB.dotcontrol");
            config.Markup.AddMarkupControl("cc", "Counter", "Controls/Counter.dotcontrol");
        }

        private void ConfigureResources(DotvvmConfiguration config, string applicationPath)
        {
            // register custom resources and adjust paths to the built-in resources
        }

		public void ConfigureServices(IDotvvmServiceCollection options)
        {
            options.AddDefaultTempStorages("temp");
            options.AddUploadedFileStorage("App_Data/Temp");
            options.AddSeleniumGenerator(o =>
            {
                //o.AddCustomGenerator(new ControlBSeleniumGenerator());
            });
        }
    }
}
