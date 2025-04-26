namespace DotVVM.Sitemap.Options;

public class SitemapOptions
{
    /// <summary>
    /// Gets the site public URL for the sitemap.
    /// If not set, it will be detected from the HTTP request.
    /// </summary>
    public Uri? SitePublicUrl { get; set; }
}
