using System;
using DotVVM.Framework.Configuration;
using DotVVM.Sitemap.Options;
using DotVVM.Sitemap.Presenters;
using DotVVM.Sitemap.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace DotVVM.Framework.Routing;

public static class DotvvmServiceCollectionExtensions
{
    /// <summary>
    /// Registers the services required for automatic generation of sitemaps for the applications.
    /// </summary>
    public static void AddSitemap(this IDotvvmServiceCollection services, Action<SitemapOptions>? configureOptions = null)
    {
        services.Services.AddScoped<SitemapPresenter>();
        services.Services.AddOptions<SitemapOptions>()
            .Configure(sitemapOptions =>
            {
                configureOptions?.Invoke(sitemapOptions);
            });
        services.Services.AddOptions<DotvvmConfiguration>()
            .Configure<IOptions<SitemapOptions>>((dotvvmConfig, sitemapOptions) =>
            {
                if (sitemapOptions.Value.AutoConfigureSitemapRoute)
                {
                    dotvvmConfig.RouteTable.Add(sitemapOptions.Value.SitemapRouteName, sitemapOptions.Value.SitemapRouteUrl, typeof(SitemapPresenter))
                        .WithSitemapOptions(sitemap => sitemap.Exclude = true);
                }
            });
        
        services.Services.AddScoped<SitemapResolver>();
        services.Services.AddScoped<SitemapXmlBuilder>();
    }
}
