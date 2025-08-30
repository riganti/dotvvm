using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;
using DotVVM.Samples.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace DotVVM.Samples.BasicSamples
{
    public class DotvvmServiceConfigurator : IDotvvmServiceConfigurator
    {
        public void ConfigureServices(IDotvvmServiceCollection services)
        {
            CommonConfiguration.ConfigureServices(services);
            services.AddDefaultTempStorages("Temp");
            services.AddHotReload();
            services.AddSitemap(opt => opt.SitemapRouteName = "Sitemap");

            services.Services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(c => c.AddConsole()));
            services.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }
    }
}
