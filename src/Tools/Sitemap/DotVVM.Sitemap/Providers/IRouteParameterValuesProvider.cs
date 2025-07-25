using DotVVM.Framework.Routing;

namespace DotVVM.Sitemap.Providers;

/// <summary>
/// Provides all possible parameter values for routes in the sitemap.
/// </summary>
public interface IRouteParameterValuesProvider
{
    /// <summary>
    /// Returns a dictionary of parameter values for each route and all its cultures (if the route is localized).
    /// </summary>
    /// <param name="routes">A collection of routes and their cultures for which the parameter values should be resolved.</param>
    Task<IReadOnlyList<SitemapRouteInstance>> GetParameterValuesAsync(IReadOnlyList<SitemapRouteProviderInput> routes, CancellationToken ct);
}

/// <summary>
/// Represents the route and a list of cultures that shall be resolved for the <see cref="IRouteParameterValuesProvider"/>.
/// </summary>
/// <param name="Route">The route for which the parameters should be resolved.</param>
/// <param name="Cultures">A list of cultures for which the parameters should be resolved. If the route is not localizable, this parameter is null.</param>
public record SitemapRouteProviderInput(RouteBase Route, IReadOnlyList<string>? Cultures);
