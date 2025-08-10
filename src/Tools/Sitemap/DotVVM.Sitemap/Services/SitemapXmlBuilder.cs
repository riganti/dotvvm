using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DotVVM.Sitemap.Options;
using DotVVM.Sitemap.Providers;
using Microsoft.Extensions.Options;

namespace DotVVM.Sitemap.Services;

public class SitemapXmlBuilder
{
    private readonly IOptions<SitemapOptions> sitemapOptions;
    private readonly XNamespace ns;
    private readonly XNamespace xhtmlns;

    public SitemapXmlBuilder(IOptions<SitemapOptions> sitemapOptions)
    {
        this.sitemapOptions = sitemapOptions;
        ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
        xhtmlns = XNamespace.Get("http://www.w3.org/1999/xhtml");
    }

    public virtual XDocument BuildXml(List<SitemapEntry> entries, Uri publicUri)
    {
        return new XDocument(new XElement(ns + "urlset", entries.SelectMany(GenerateUrlElements)));
    }

    private IEnumerable<XElement> GenerateUrlElements(SitemapEntry entry)
    {
        if (entry.LocalizedUrls == null)
        {
            yield return GenerateUrlElement(entry.Url, entry.Options, null);
        }
        else
        {
            var localizedUrls = new[] { (Culture: sitemapOptions.Value.DefaultCulture, Url: entry.Url) }
                .Concat(entry.LocalizedUrls.Select(u => (Culture: u.Key, Url: u.Value)))
                .ToList();

            foreach (var culture in localizedUrls)
            {
                yield return GenerateUrlElement(culture.Url, entry.Options, localizedUrls);
            }
        }
    }

    private XElement GenerateUrlElement(string url, RouteSitemapOptions options, IEnumerable<(string Culture, string Url)>? alternateCultureUrls)
    {
        return new XElement(ns + "url",
            new XElement(ns + "loc", url),
            options.Priority != null ? new XElement(ns + "priority", FormatPriority(options.Priority.Value)) : null,
            options.LastModified != null ? new XElement(ns + "lastmod", FormatLastModified(options.LastModified.Value)) : null,
            options.ChangeFrequency != null ? new XElement(ns + "changefreq", FormatChangeFrequency(options.ChangeFrequency.Value)) : null,
            alternateCultureUrls?.Select(a => new XElement(xhtmlns + "link",
                new XAttribute("rel", "alternate"),
                new XAttribute("hreflang", a.Culture),
                new XAttribute("href", a.Url))
            )
        );
    }

    private string FormatChangeFrequency(ChangeFrequency changeFrequency) => changeFrequency.ToString().ToLowerInvariant();

    private string FormatLastModified(DateTime lastModified) => lastModified.ToString("yyyy-MM-ddTHH:mm:sszzz");

    private string FormatPriority(double priority) => priority.ToString("n1", CultureInfo.InvariantCulture);
}
