using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore
{
    public class DotvvmServiceConfigurator : IDotvvmServiceConfigurator
    {
        public void ConfigureServices(IDotvvmOptions options)
        {
            options.AddDefaultTempStorages("temp");
        }
    }
}
