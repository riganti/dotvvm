using DotVVM.Framework.Routing;
using DotVVM.Sitemap.Options;

namespace DotVVM.Sitemap.Providers;

/// <summary>
/// Represents a context for resolving sitemap entries for a particular route, its parameters, and cultures (if the route is localized).
/// </summary>
public class RouteSitemapContext
{
    private readonly Uri publicUrl;

    /// <summary>
    /// Gets the route for which the sitemap entries are being resolved.
    /// </summary>
    public RouteBase Route { get; }

    /// <summary>
    /// Gets the options for the route, which can include priority, change frequency, last modification date, etc.
    /// </summary>
    public RouteSitemapOptions Options { get; }

    /// <summary>
    /// Gets the list of cultures for which the route is localized, or null if the route is not localized.
    /// </summary>
    public IReadOnlyList<string>? Cultures { get; }

    /// <summary>
    /// Gets whether the route is localized (i.e., has multiple cultures).
    /// </summary>
    public bool IsLocalized => Cultures != null;

    /// <summary>
    /// Gets the names of the parameters for the route whose values need to be resolved.
    /// </summary>
    public IReadOnlyList<string> ParameterNames { get; }

    private readonly List<SitemapEntry> entries = new();

    /// <summary>
    /// Gets the list of sitemap entries that have been added for the route.
    /// </summary>
    public IReadOnlyList<SitemapEntry> Entries => entries;

    /// <summary>
    /// Initializes a new instance of the <see cref="RouteSitemapContext"/> class for a given route.
    /// </summary>
    public RouteSitemapContext(RouteBase route, RouteSitemapOptions options, Uri publicUrl)
    {
        this.publicUrl = publicUrl;

        Route = route ?? throw new ArgumentNullException(nameof(route), "Route cannot be null.");
        Options = options ?? throw new ArgumentNullException(nameof(options), "Route sitemap options cannot be null.");

        if (route is LocalizedDotvvmRoute localizedRoute)
        {
            Cultures = localizedRoute.GetAllCultureRoutes().Keys.Where(k => k != "").ToList();
        }
        else
        {
            Cultures = null;
        }
        ParameterNames = route.ParameterNames.ToList();
    }

    /// <summary>
    /// Builds a sitemap entry from the specified parameter values (and their localized versions, if the route is localized)
    /// </summary>
    /// <param name="parameterValues">An object or a dictionary that contains values of route parameters.</param>
    /// <param name="localizedParameterValues">A dictionary of culture and <paramref cref="parameterValues" /> pairs, or null, if the route is not localized.</param>
    /// <param name="overrideOptions">An options object that can override priority, change frequency or last modification time.</param>
    public void AddSitemapEntry(object parameterValues, Dictionary<string, object>? localizedParameterValues = null, RouteSitemapOptions? overrideOptions = null)
    {
        if (!IsLocalized && localizedParameterValues != null)
        {
            throw new ArgumentNullException(nameof(localizedParameterValues), "Localized parameter values cannot be provided for non-localized routes.");
        }

        if (IsLocalized && localizedParameterValues == null)
        {
            // if parameterValues are not provided for localized routes, use the same values for all cultures
            localizedParameterValues = Cultures!.ToDictionary(c => c, _ => parameterValues);
        }

        try
        {
            var url = BuildPublicUrl(Route, parameterValues);
            var localizedUrls = localizedParameterValues?.ToDictionary(
                e => e.Key,
                e => BuildPublicUrl(((LocalizedDotvvmRoute)Route).GetRouteForCulture(e.Key), e.Value));

            entries.Add(new SitemapEntry(url, localizedUrls, Options.CreateDerivedOptions(overrideOptions)));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create sitemap entry for route '{Route.RouteName}'. Ensure that the supplied parameter names correspond to the route parameters ({string.Join(", ", ParameterNames)}){(IsLocalized ? $" and route culture names ({string.Join(", ", Cultures!)})" : "")}. See inner exception for details.", ex);
        }
    }

    private string BuildPublicUrl(RouteBase route, object parameterValues)
    {
        return new Uri(publicUrl, route.BuildUrl(parameterValues).TrimStart('~', '/')).ToString();
    }
}
