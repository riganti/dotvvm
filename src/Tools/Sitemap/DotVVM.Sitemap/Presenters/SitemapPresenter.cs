using System.Globalization;
using System.Xml.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;
using DotVVM.Sitemap.Options;
using DotVVM.Sitemap.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotVVM.Sitemap.Presenters;

public class SitemapPresenter(DotvvmConfiguration configuration, IOptions<SitemapOptions> options) : IDotvvmPresenter
{
    public async Task ProcessRequest(IDotvvmRequestContext context)
    {
        var ct = context.GetCancellationToken();

        // determine public URL
        var publicUrl = GetPublicUrl(context);

        // get entries
        var entries = await GetSitemapEntriesAsync(context, publicUrl, ct);

        // write XML
        var xml = BuildXml(entries);
        context.HttpContext.Response.ContentType = "application/xml";
        await xml.SaveAsync(context.HttpContext.Response.Body, SaveOptions.None, ct);
    }

    private Uri GetPublicUrl(IDotvvmRequestContext context)
    {
        if (options.Value.SitePublicUrl != null)
        {
            return options.Value.SitePublicUrl;
        }

        // if not set, use the URL from the request
        var uri = new UriBuilder(context.HttpContext.Request.Url);
        uri.Path = context.HttpContext.Request.PathBase.Value ?? "";
        return uri.Uri;
    }

    private XDocument BuildXml(List<SitemapEntry> entries)
    {
        var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
        var xhtmlns = XNamespace.Get("http://www.w3.org/1999/xhtml");

        return new XDocument(
            new XElement(ns + "urlset",
                entries.Select(e => new XElement(ns + "url",
                    new XElement(ns + "loc", e.Url),
                    e.Priority != null ? new XElement(ns + "priority", e.Priority.Value.ToString("n1", CultureInfo.InvariantCulture)) : null,
                    e.LastModified != null ? new XElement(ns + "lastmod", e.LastModified.Value.ToString("yyyy-MM-ddTHH:mm:sszzz")) : null,
                    e.ChangeFrequency != ChangeFrequency.Always ? new XElement(ns + "changefreq", e.ChangeFrequency.ToString().ToLowerInvariant()) : null,
                    e.AlternateCultureUrls?.Select(a => new XElement(xhtmlns + "link",
                        new XAttribute("rel", "alternate"),
                        new XAttribute("hreflang", a.Key),
                        new XAttribute("href", a.Value))
                    )
                ))
            )
        );
    }

    private async Task<List<SitemapEntry>> GetSitemapEntriesAsync(IDotvvmRequestContext context, Uri publicUri, CancellationToken ct)
    {
        // find routes with RouteSitemapOptions
        var routes = configuration.RouteTable
            .Select(r => (Route: r, SitemapOptions: TryGetRouteSitemapOptions(r)))
            .Where(r => r.SitemapOptions != null && r.SitemapOptions.Exclude != true)
            .Where(r => !r.Route.Url.StartsWith("_dotvvm"))
            .Select(r => (r.Route, SitemapOptions: r.SitemapOptions!));

        // expand parameters
        var sitemapEntries = new List<SitemapEntry>();
        foreach (var route in routes)
        {
            if (route.Route is LocalizedDotvvmRoute localizedRoute)
            {
                // obtain entries for each culture
                var localizedEntries = new List<(int RouteInstanceIndex, SitemapEntry Entry)>();
                foreach (var routeWithCulture in localizedRoute.GetAllCultureRoutes())
                {
                    localizedEntries.AddRange(await BuildRouteEntriesAsync(context, publicUri, routeWithCulture.Value, routeWithCulture.Key, route.SitemapOptions, ct));
                }

                // generate alternate links
                foreach (var routeInstanceEntries in localizedEntries.GroupBy(e => e.RouteInstanceIndex))
                {
                    foreach (var routeInstanceEntry in routeInstanceEntries)
                    {
                        routeInstanceEntry.Entry.AlternateCultureUrls = routeInstanceEntries
                            .Where(e => e != routeInstanceEntry)
                            .ToDictionary(e => e.Entry.Culture, e => e.Entry.Url);
                    }
                }

                sitemapEntries.AddRange(localizedEntries.Select(e => e.Entry));
            }
            else
            {
                // single culture route
                var routeEntries = await BuildRouteEntriesAsync(context, publicUri, route.Route, null, route.SitemapOptions, ct);
                sitemapEntries.AddRange(routeEntries.Select(e => e.Entry));
            }

            ct.ThrowIfCancellationRequested();
        }

        return sitemapEntries;
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

    private async Task<IEnumerable<(int RouteInstanceIndex, SitemapEntry Entry)>> BuildRouteEntriesAsync(IDotvvmRequestContext context, Uri publicUri, RouteBase route, string? culture, RouteSitemapOptions sitemapOptions, CancellationToken ct)
    {
        var entries = new List<(int RouteInstanceIndex, SitemapEntry Entry)>();
        var lastModifiedProvider = context.Services.GetRequiredService(sitemapOptions.LastModifiedProviderType ?? typeof(AppDeploymentTimeLastModificationDateProvider));

        if (sitemapOptions.ParameterValuesProviderType is { } providerType)
        {
            // there are multiple pages with different content for the same route
            var provider = context.Services.GetRequiredService(providerType);

            var routeInstances = await ((IRouteParameterValuesProvider)provider).GetParameterValuesAsync(route, culture, ct);
            ct.ThrowIfCancellationRequested();

            for (var index = 0; index < routeInstances.Count; index++)
            {
                var routeInstance = routeInstances[index];
                var pageUrl = route.BuildUrl(routeInstance.ParameterValues).TrimStart('~', '/');
                var entry = new SitemapEntry()
                {
                    Url = new Uri(publicUri, pageUrl).ToString(),
                    Culture = string.IsNullOrEmpty(culture) ? "x-default" : culture,
                    ChangeFrequency = sitemapOptions.ChangeFrequency ?? ChangeFrequency.Always,
                    LastModified = await GetLastModifiedDateAsync(routeInstance),
                    Priority = sitemapOptions.Priority ?? 1d
                };
                entries.Add((index, entry));
            }
        }
        else
        {
            // single instance of the route
            var entry = new SitemapEntry()
            {
                Url = new Uri(publicUri, route.BuildUrl().TrimStart('~', '/')).ToString(),
                Culture = string.IsNullOrEmpty(culture) ? "x-default" : culture,
                ChangeFrequency = sitemapOptions.ChangeFrequency ?? ChangeFrequency.Always,
                LastModified = await GetLastModifiedDateAsync(null),
                Priority = sitemapOptions.Priority ?? 1d
            };
            entries.Add((0, entry));
        }

        return entries;

        async Task<DateTime> GetLastModifiedDateAsync(RouteInstanceData? routeInstanceData)
        {
            if (routeInstanceData is { LastModifiedDate: not null })
            {
                return routeInstanceData.LastModifiedDate.Value;
            }
            else if (sitemapOptions.LastModified != null)
            {
                return sitemapOptions.LastModified.Value;
            }

            return await ((IRouteLastModificationDateProvider)lastModifiedProvider).GetLastModifiedTimeAsync(route, culture, routeInstanceData?.ParameterValues, ct);
        }
    }
}

internal class SitemapEntry
{
    public required string Url { get; set; }
    public required string Culture { get; set; }
    public DateTime? LastModified { get; set; }
    public ChangeFrequency ChangeFrequency { get; set; }
    public Dictionary<string, string>? AlternateCultureUrls { get; set; }
    public double? Priority { get; set; }
}
