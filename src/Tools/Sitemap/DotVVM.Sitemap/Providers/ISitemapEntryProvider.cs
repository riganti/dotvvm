namespace DotVVM.Sitemap.Providers;

/// <summary>
/// Represents a provider that resolves sitemap entries for routes with parameters.
/// </summary>
public interface ISitemapEntryProvider
{

    /// <summary>
    /// Resolves sitemap entries for the parameterized routes.
    /// </summary>
    Task TryResolveSitemapEntries(IReadOnlyList<RouteSitemapContext> routes, CancellationToken ct);

}
