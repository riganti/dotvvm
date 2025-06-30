using DotVVM.Sitemap.Options;
using DotVVM.Sitemap.Presenters;
using DotVVM.Sitemap.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Routing;

public static class DotvvmServiceCollectionExtensions
{
    public static void AddSitemapServices(this IDotvvmServiceCollection services, Action<SitemapOptions>? configureOptions = null)
    {
        services.Services.AddSingleton<SitemapPresenter>();
        services.Services.AddOptions<SitemapOptions>().Configure(configureOptions ?? (_ => { }));
        services.Services.AddSingleton<AppDeploymentTimeLastModificationDateProvider>();
    }
}
