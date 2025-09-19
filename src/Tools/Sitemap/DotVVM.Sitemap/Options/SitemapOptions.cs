using System;

namespace DotVVM.Sitemap.Options;

public class SitemapOptions
{
    /// <summary>
    /// Gets the site public URL for the sitemap.
    /// If not set, it will be detected from the HTTP request.
    /// </summary>
    public Uri? SitePublicUrl { get; set; }

    /// <summary>
    /// Gets or sets the default culture for the sitemap.
    /// This culture is used in the generated sitemap instead of x-default: https://developers.google.com/search/docs/specialty/international/localized-versions#sitemap
    /// </summary>
    public string DefaultCulture { get; set; } = "x-default";

    /// <summary>
    /// Gets or sets the name of the route that serves the sitemap XML.
    /// </summary>
    public string SitemapRouteName { get; set; } = "__dotvvm_sitemap";

    /// <summary>
    /// Gets or sets the URL of the sitemap XML (relative to the site root).
    /// </summary>
    public string SitemapRouteUrl { get; set; } = "sitemap.xml";

    /// <summary>
    /// Gets or sets whether the sitemap route should be automatically configured using <see cref="SitemapRouteName"/> and <see cref="SitemapRouteUrl"/> properties.
    /// </summary>
    public bool AutoConfigureSitemapRoute { get; set; } = true;
}
