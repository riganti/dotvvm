using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;
using DotVVM.Sitemap.Options;
using DotVVM.Sitemap.Providers;

namespace DotVVM.Sitemap.Services;

public class SitemapResolver(DotvvmConfiguration configuration, IEnumerable<ISitemapEntryProvider> sitemapEntryProviders)
{
    public virtual async Task<List<SitemapEntry>> GetSitemapEntriesAsync(IDotvvmRequestContext context, Uri publicUrl, CancellationToken ct)
    {
        var routeContexts = BuildRouteContexts(publicUrl);

        // automatically add sitemap entries for non-parameterized routes
        foreach (var routeContext in routeContexts.Where(c => c.ParameterNames.Count == 0))
        {
            routeContext.AddSitemapEntry(new object() {});
        }

        // call providers to resolve other entries
        foreach (var provider in sitemapEntryProviders)
        {
            await provider.TryResolveSitemapEntries(routeContexts, ct);
        }

        // make sure all routes have at least one entry
        var routesWithoutEntries = routeContexts
            .Where(c => c.Entries.Count == 0)
            .ToList();
        if (routesWithoutEntries.Any())
        {
            context.DebugWarning($"The following parameterized routes do not have any sitemap entries: {string.Join(", ", routesWithoutEntries.Select(c => c.Route.RouteName))}. " +
                                 "Ensure that the corresponding ISitemapEntryProviders are properly registered, or make sure the routes are excluded.");
        }

        return routeContexts
            .SelectMany(c => c.Entries)
            .ToList();
    }

    private List<RouteSitemapContext> BuildRouteContexts(Uri publicUrl)
    {
        // find routes with RouteSitemapOptions
        var routes = configuration.RouteTable
            .Select(r => (Route: r, SitemapOptions: TryGetRouteSitemapOptions(r)))
            .Where(r => r.SitemapOptions != null && r.SitemapOptions.Exclude != true)
            .Where(r => !r.Route.Url.StartsWith("_dotvvm"))
            .Select(r => (
                r.Route,
                SitemapOptions: r.SitemapOptions!,
                Cultures: r.Route is LocalizedDotvvmRoute localizedDotvvmRoute ? localizedDotvvmRoute.GetAllCultureRoutes().Keys.ToArray() : null
            ))
            .ToList();

        return routes
            .Select(r => new RouteSitemapContext(r.Route, r.SitemapOptions, publicUrl))
            .ToList();
    }

    private RouteSitemapOptions? TryGetRouteSitemapOptions(RouteBase route)
    {
        var chain = GetRouteSitemapOptionsChain(route).Reverse();

        RouteSitemapOptions? result = null;
        foreach (var options in chain)
        {
            result = (result ?? new RouteSitemapOptions()).CreateDerivedOptions(options);
        }
        return result;
    }

    private IEnumerable<RouteSitemapOptions> GetRouteSitemapOptionsChain(RouteBase route)
    {
        if (route.ExtensionData.TryGetValue(typeof(RouteSitemapOptions), out var o) && o is RouteSitemapOptions routeOptions)
        {
            yield return routeOptions;
        }
        if (route.VirtualPath == null)
        {
            // routes without virtual path are excluded by default if they do not specify options explicitly
            yield return new RouteSitemapOptions() { Exclude = true };
        }

        var parentGroup = route.ParentRouteGroup;
        while (parentGroup != null)
        {
            if (parentGroup.ExtensionData.TryGetValue(typeof(RouteSitemapOptions), out var o2) && o2 is RouteSitemapOptions groupOptions)
            {
                yield return groupOptions;
            }
            parentGroup = parentGroup.ParentRouteGroup;
        }

        if (configuration.RouteTable.ExtensionData.TryGetValue(typeof(RouteSitemapOptions), out var o3) && o3 is RouteSitemapOptions routeTableOptions)
        {
            yield return routeTableOptions;
        }
    }
}
