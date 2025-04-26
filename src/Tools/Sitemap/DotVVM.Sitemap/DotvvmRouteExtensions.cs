using DotVVM.Sitemap.Options;
using DotVVM.Sitemap.Presenters;

namespace DotVVM.Framework.Routing;

public static class DotvvmRouteExtensions
{
    public static RouteBase WithSitemap(this RouteBase route, Action<RouteSitemapOptions> configure)
    {
        var options = new RouteSitemapOptions();
        configure(options);
        route.ExtensionData.Add(typeof(RouteSitemapOptions), options);

        return route;
    }

    public static RouteBase AddSitemapRoute(this DotvvmRouteTable routeTable)
    {
        return routeTable.Add("Sitemap", "sitemap.xml", typeof(SitemapPresenter));
    }
}
