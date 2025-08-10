using DotVVM.Sitemap.Options;
using DotVVM.Sitemap.Presenters;
using DotVVM.Sitemap.Services;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace DotVVM.Framework.Routing;

public static class DotvvmServiceCollectionExtensions
{
    /// <summary>
    /// Registers the services required for automatic generation of sitemaps for the applications.
    /// </summary>
    public static void AddSitemapServices(this IDotvvmServiceCollection services, Action<SitemapOptions>? configureOptions = null)
    {
        services.Services.AddScoped<SitemapPresenter>();
        services.Services.AddOptions<SitemapOptions>().Configure(configureOptions ?? (_ => { }));

        services.Services.AddScoped<SitemapResolver>();
        services.Services.AddScoped<SitemapXmlBuilder>();
    }
}
