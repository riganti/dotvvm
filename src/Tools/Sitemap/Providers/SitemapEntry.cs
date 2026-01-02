using System.Collections.Generic;
using DotVVM.Sitemap.Options;

namespace DotVVM.Sitemap.Providers;

/// <summary>
/// Represents a single entry in the sitemap, including the URL, localized URLs (if applicable), and optional override options.
/// </summary>
public record SitemapEntry(string Url, Dictionary<string, string>? LocalizedUrls, RouteSitemapOptions Options);
