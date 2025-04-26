using DotVVM.Framework.Routing;

namespace DotVVM.Sitemap.Providers;

public interface IRouteParameterValuesProvider
{
    Task<Dictionary<string, object?>> GetParameterValuesAsync(RouteBase route, string? culture, CancellationToken ct);
}
