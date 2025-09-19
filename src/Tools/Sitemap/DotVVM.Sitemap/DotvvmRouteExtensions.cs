using System;
using DotVVM.Sitemap.Options;
using DotVVM.Sitemap.Presenters;

// ReSharper disable once CheckNamespace
namespace DotVVM.Framework.Routing;

public static class DotvvmRouteExtensions
{
    public static RouteBase WithSitemapOptions(this RouteBase route, Action<RouteSitemapOptions>? configure = null)
    {
        if (!route.ExtensionData.TryGetValue(typeof(RouteSitemapOptions), out var options))
        {
            options = new RouteSitemapOptions();
            route.ExtensionData.Add(typeof(RouteSitemapOptions), options);
        }
        configure?.Invoke(options as RouteSitemapOptions ?? throw new InvalidOperationException("The ExtensionData dictionary item must be of type RouteSitemapOptions!"));

        return route;
    }

    public static RouteTableGroup WithDefaultSitemapOptions(this RouteTableGroup routeGroup, Action<RouteSitemapOptions>? configure = null)
    {
        if (!routeGroup.ExtensionData.TryGetValue(typeof(RouteSitemapOptions), out var options))
        {
            options = new RouteSitemapOptions();
            routeGroup.ExtensionData.Add(typeof(RouteSitemapOptions), options);
        }
        configure?.Invoke(options as RouteSitemapOptions ?? throw new InvalidOperationException("The ExtensionData dictionary item must be of type RouteSitemapOptions!"));

        return routeGroup;
    }

    public static DotvvmRouteTable WithDefaultSitemapOptions(this DotvvmRouteTable routeTable, Action<RouteSitemapOptions>? configure = null)
    {
        if (!routeTable.ExtensionData.TryGetValue(typeof(RouteSitemapOptions), out var options))
        {
            options = new RouteSitemapOptions();
            routeTable.ExtensionData.Add(typeof(RouteSitemapOptions), options);
        }
        configure?.Invoke(options as RouteSitemapOptions ?? throw new InvalidOperationException("The ExtensionData dictionary item must be of type RouteSitemapOptions!"));

        return routeTable;
    }
}
