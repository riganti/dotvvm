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
            .Select(r => (
                r.Route,
                SitemapOptions: r.SitemapOptions!,
                Cultures: r.Route is LocalizedDotvvmRoute localizedDotvvmRoute ? localizedDotvvmRoute.GetAllCultureRoutes().Keys.ToArray() : null
            ))
            .ToList();

        // resolve last modified dates
        var routeLastModifiedDates = new Dictionary<RouteBase, DateTime?>();
        foreach (var routeGroup in routes.GroupBy(r => r.SitemapOptions.LastModifiedProviderType))
        {
            if (routeGroup.Key == null)
            {
                // no last modified provider, so we use the default value
                continue;
            }

            var lastModifiedProvider = context.Services.GetRequiredService(routeGroup.Key);
            foreach (var route in routeGroup)
            {
                var lastModifiedDate = await ((IRouteLastModificationDateProvider)lastModifiedProvider).GetLastModifiedTimeAsync(route.Route, ct);
                if (lastModifiedDate != null)
                {
                    routeLastModifiedDates[route.Route] = lastModifiedDate;
                }
            }
        }

        // expand parameters
        var sitemapEntries = new List<SitemapEntry>();
        foreach (var routeGroup in routes.GroupBy(r => r.SitemapOptions.ParameterValuesProviderType))
        {
            IReadOnlyList<SitemapRouteInstance> routeInstances;
            if (routeGroup.Key == null)
            {
                // routes have no parameter values provider, so they are single instances
                routeInstances = routeGroup
                    .Select(r =>
                        new SitemapRouteInstance(
                            r.Route,
                            ((string?[]?)r.Cultures ?? [null])
                                .Select(c => new SitemapRouteInstanceParametersData(c, new Dictionary<string, object?>()))
                                .ToList(),
                            null)
                    )
                    .ToList();
            }
            else
            {
                // obtain values from the provider
                var provider = context.Services.GetRequiredService(routeGroup.Key);
                var routeInputs = routeGroup.Select(r => new SitemapRouteProviderInput(r.Route, r.Cultures)).ToArray();
                routeInstances = await ((IRouteParameterValuesProvider)provider).GetParameterValuesAsync(routeInputs, ct);
            }
            
            foreach (var routeInstance in routeInstances)
            {
                var urls = routeInstance.Parameters
                    .Select(p => {
                        var route = p.Culture != null
                            ? ((LocalizedDotvvmRoute)routeInstance.Route).GetRouteForCulture(p.Culture)
                            : routeInstance.Route;
                        return new Uri(publicUri, route.BuildUrl(p.ParameterValues).TrimStart('~', '/')).ToString();
                    })
                    .ToList();

                var sitemapOptions = routeGroup.Single(g => g.Route == routeInstance.Route).SitemapOptions;
                for (var i = 0; i < routeInstance.Parameters.Count; i++)
                {
                    sitemapEntries.Add(new SitemapEntry()
                    {
                        Url = new Uri(publicUri, urls[i]).ToString(),
                        Culture = routeInstance.Parameters[i].Culture ?? "x-default",
                        ChangeFrequency = sitemapOptions.ChangeFrequency ?? ChangeFrequency.Always,
                        LastModified = routeInstance.LastModifiedDate
                                       ?? (routeLastModifiedDates.TryGetValue(routeInstance.Route, out var lastModifiedDate) ? lastModifiedDate : null),
                        Priority = sitemapOptions.Priority ?? 1d,
                        AlternateCultureUrls = Enumerable.Range(0, routeInstance.Parameters.Count)
                            .Where(p => p != i)
                            .ToDictionary(
                                p => routeInstance.Parameters[p].Culture!,
                                p => urls[p]
                            )
                    });
                }

                ct.ThrowIfCancellationRequested();
            }
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
