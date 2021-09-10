using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Samples.ApplicationInsights.AspNetCore
{
    public class DotvvmStartup : IDotvvmStartup, IDotvvmServiceConfigurator
    {
        public void ConfigureServices(IDotvvmServiceCollection options)
        {
            options
                .AddDefaultTempStorages("temp")
                .AddApplicationInsightsTracing();
        }

        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            ConfigureRoutes(config, applicationPath);
        }

        private void ConfigureRoutes(DotvvmConfiguration config, string applicationPath)
        {
            config.RouteTable.Add("Default", "", "Views/default.dothtml");

            config.RouteTable.Add("InitException", "Test/InitException", "Views/Test/initException.dothtml");
            config.RouteTable.Add("CommandException", "Test/CommandException", "Views/Test/commandException.dothtml");
            config.RouteTable.Add("Correct", "Test/Correct", "Views/Test/correct.dothtml");
            config.RouteTable.Add("CorrectCommand", "Test/CorrectCommand", "Views/Test/correctCommand.dothtml");
        }
    }
}
