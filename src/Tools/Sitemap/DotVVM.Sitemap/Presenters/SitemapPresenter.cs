using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;
using DotVVM.Sitemap.Options;
using DotVVM.Sitemap.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotVVM.Sitemap.Presenters;

public class SitemapPresenter(DotvvmConfiguration configuration) : IDotvvmPresenter
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

        var writer = new XmlTextWriter(context.HttpContext.Response.Body, System.Text.Encoding.UTF8) {
            Formatting = Formatting.Indented,
            Indentation = 2
        };
        await xml.WriteToAsync(writer, ct);
    }

    private Uri GetPublicUrl(IDotvvmRequestContext context)
    {
        var options = context.Services.GetRequiredService<IOptions<SitemapOptions>>();
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
                    e.LastModified != null ? new XElement(ns + "lastmod", e.LastModified.Value.ToString("o")) : null,
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
            .Select(r => (Route: r, SitemapOptions: r.ExtensionData.TryGetValue(typeof(RouteSitemapOptions), out var o) ? o : null))
            .Where(r => r.SitemapOptions != null)
            .Select(r => (r.Route, SitemapOptions: (RouteSitemapOptions)r.SitemapOptions!));

        // expand parameters
        var entries = new List<SitemapEntry>();
        foreach (var route in routes)
        {
            if (route.Route is LocalizedDotvvmRoute localizedRoute)
            {
                // obtain entries for each culture
                var localizedEntries = new List<SitemapEntry>();
                foreach (var routeWithCulture in localizedRoute.GetAllCultureRoutes())
                {
                    localizedEntries.AddRange(await BuildRouteEntriesAsync(context, publicUri, routeWithCulture.Value, routeWithCulture.Key, route.SitemapOptions, ct));
                }

                // generate alternate links
                foreach (var entry in localizedEntries)
                {
                    entry.AlternateCultureUrls = localizedEntries
                        .Where(e => e != entry)
                        .ToDictionary(e => e.Culture, e => e.Url);
                }

                entries.AddRange(localizedEntries);
            }
            else
            {
                // single culture route
                entries.AddRange(await BuildRouteEntriesAsync(context, publicUri, route.Route, null, route.SitemapOptions, ct));
            }

            ct.ThrowIfCancellationRequested();
        }

        return entries;
    }

    private async Task<IEnumerable<SitemapEntry>> BuildRouteEntriesAsync(IDotvvmRequestContext context, Uri publicUri, RouteBase route, string? culture, RouteSitemapOptions sitemapOptions, CancellationToken ct)
    {
        var entries = new List<SitemapEntry>();
        var lastModifiedProvider = context.Services.GetRequiredService(sitemapOptions.LastModifiedProviderType);

        if (sitemapOptions.ParameterValuesProviderType is { } providerType)
        {
            // there are multiple pages with different content for the same route
            var provider = context.Services.GetRequiredService(providerType);

            var parameterValues = await ((IRouteParameterValuesProvider)provider).GetParameterValuesAsync(route, culture, ct);
            ct.ThrowIfCancellationRequested();

            foreach (var parameterValue in parameterValues)
            {
                var pageUrl = route.BuildUrl(parameterValue).TrimStart('~', '/');
                entries.Add(new SitemapEntry()
                {
                    Url = new Uri(publicUri, pageUrl).ToString(),
                    Culture = string.IsNullOrEmpty(culture) ? "x-default" : culture,
                    ChangeFrequency = sitemapOptions.ChangeFrequency,
                    LastModified = await GetLastModifiedDateAsync(parameterValues),
                });
            }
        }
        else
        {
            // single instance of the route
            entries.Add(new SitemapEntry()
            {
                Url = route.BuildUrl(),
                Culture = string.IsNullOrEmpty(culture) ? "x-default" : culture,
                ChangeFrequency = sitemapOptions.ChangeFrequency,
                LastModified = await GetLastModifiedDateAsync(null)
            });
        }

        return entries;

        async Task<DateTime> GetLastModifiedDateAsync(Dictionary<string, object?>? parameterValues)
        {
            if (sitemapOptions.LastModified != null)
            {
                return sitemapOptions.LastModified.Value;
            }

            return await ((IRouteLastModificationDateProvider)lastModifiedProvider).GetLastModifiedTimeAsync(route, culture, parameterValues, ct);
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
