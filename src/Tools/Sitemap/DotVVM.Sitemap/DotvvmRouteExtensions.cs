using DotVVM.Framework.Configuration;
using DotVVM.Sitemap.Options;
using DotVVM.Sitemap.Presenters;

namespace DotVVM.Framework.Routing;

public static class DotvvmRouteExtensions
{
    public static RouteBase WithSitemapOptions(this RouteBase route, Action<RouteSitemapOptions> configure)
    {
        var options = new RouteSitemapOptions();
        configure(options);
        route.ExtensionData.Add(typeof(RouteSitemapOptions), options);

        return route;
    }

    public static RouteTableGroup WithDefaultSitemapOptions(this RouteTableGroup routeGroup, Action<RouteSitemapOptions> configure)
    {
        var options = new RouteSitemapOptions();
        configure(options);
        routeGroup.ExtensionData.Add(typeof(RouteSitemapOptions), options);

        return routeGroup;
    }

    public static DotvvmRouteTable WithDefaultSitemapOptions(this DotvvmRouteTable routeTable, Action<RouteSitemapOptions> configure)
    {
        var options = new RouteSitemapOptions();
        configure(options);
        routeTable.ExtensionData.Add(typeof(RouteSitemapOptions), options);

        return routeTable;
    }

    public static RouteBase AddSitemapRoute(this DotvvmRouteTable routeTable)
    {
        return routeTable.Add("Sitemap", "sitemap.xml", typeof(SitemapPresenter))
            .WithSitemapOptions(sitemap => sitemap.Exclude = true);
    }
}
