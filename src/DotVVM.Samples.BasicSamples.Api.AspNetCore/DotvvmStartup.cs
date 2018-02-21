using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore
{
    public class DotvvmStartup : IDotvvmStartup
    {
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            config.RouteTable.Add("Generator", "", "Views/Generator.dothtml");
        }

        public void ConfigureServices(IDotvvmServiceCollection serviceCollection)
        {
                serviceCollection.AddDefaultTempStorages("temp");
        }
    }
}
