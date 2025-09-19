using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;
using DotVVM.Samples.Common;
using Microsoft.Extensions.DependencyInjection;

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
        }
    }
}
