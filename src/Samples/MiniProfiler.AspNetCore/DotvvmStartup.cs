using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Samples.MiniProfiler.AspNetCore
{
    public class DotvvmStartup : IDotvvmStartup, IDotvvmServiceConfigurator
    {
        public void ConfigureServices(IDotvvmServiceCollection options)
        {
            options
                .AddDefaultTempStorages("temp")
                .AddMiniProfilerEventTracing();
        }

        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            ConfigureRoutes(config, applicationPath);
        }

        private void ConfigureRoutes(DotvvmConfiguration config, string applicationPath)
        {
            config.RouteTable.Add("Default", "", "Views/default.dotmaster");
            config.RouteTable.Add("Page1", "page1", "Views/page1.dothtml");
            config.RouteTable.Add("Page2", "page2", "Views/page2.dothtml");
        }
    }
}
