using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore
{
    public class DotvvmServiceConfigurator : IDotvvmServiceConfigurator
    {
        public void ConfigureServices(IDotvvmServiceCollection serviceCollection)
        {
            serviceCollection.AddDefaultTempStorages("temp");
        }
    }
}
