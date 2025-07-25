using DotVVM.Framework.Routing;

namespace DotVVM.Sitemap.Providers;

/// <summary>
/// Represents a dictionary of route parameter values for a particular route and its culture (if the route is localized).
/// </summary>
public class SitemapRouteInstance
{
    /// <summary>
    /// Represents a dictionary of route parameter values for a particular route and its culture (if the route is localized).
    /// </summary>
    /// <param name="route">The route to which the parameter values belong.</param>
    /// <param name="parameters">An array of parameters for each culture available for route. If the route is not localized, pass a single-element array with Culture set to null.</param>
    /// <param name="lastModifiedDate">The date of last modification of the content. Return null to use the value from a provider specified on the route.</param>
    public SitemapRouteInstance(RouteBase route, IReadOnlyList<SitemapRouteInstanceParametersData> parameters, DateTime? lastModifiedDate = null)
    {
        if (route is LocalizedDotvvmRoute localizedRoute)
        {
            var missingCultures = localizedRoute.GetAllCultureRoutes().Keys
                .Where(c => !parameters.Any(p => p.Culture == c))
                .ToList();
            if (missingCultures.Any())
            {
                throw new ArgumentException($"The route {route.RouteName} is localized to {string.Join(", ", missingCultures)}, but the provider didn't return parameters for these cultures!");
            }
        }
        else if (parameters is not [{ Culture: null }])
        {
            throw new ArgumentException("If the route is not localized, the parameters argument must contain a single element with Culture set to null.", nameof(parameters));
        }

        this.Route = route;
        this.Parameters = parameters;
        this.LastModifiedDate = lastModifiedDate;
    }

    /// <summary>The route to which the parameter values belong.</summary>
    public RouteBase Route { get; init; }

    /// <summary>An array of parameters for each culture available for route. If the route is not localized, pass a single-element array with Culture set to null.</summary>
    public IReadOnlyList<SitemapRouteInstanceParametersData> Parameters { get; init; }

    /// <summary>The date of last modification of the content. Return null to use the value from a provider specified on the route.</summary>
    public DateTime? LastModifiedDate { get; init; }
}

/// <summary>
/// Represents a dictionary of route parameter values for a particular route and its culture (if the route is localized).
/// </summary>
public record SitemapRouteInstanceParametersData(string? Culture, Dictionary<string, object?> ParameterValues);
