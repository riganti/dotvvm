using DotVVM.Framework.Routing;

namespace DotVVM.Sitemap.Providers;

public interface IRouteParameterValuesProvider
{
    Task<IReadOnlyList<RouteInstanceData>> GetParameterValuesAsync(RouteBase route, string? culture, CancellationToken ct);
}

public record RouteInstanceData(IDictionary<string, object?> ParameterValues, DateTime? LastModifiedDate = null);
