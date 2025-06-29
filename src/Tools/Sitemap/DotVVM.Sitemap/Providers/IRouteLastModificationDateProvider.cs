using DotVVM.Framework.Routing;

namespace DotVVM.Sitemap.Providers;

public interface IRouteLastModificationDateProvider
{
    Task<DateTime> GetLastModifiedTimeAsync(RouteBase route, string? culture, IDictionary<string, object?>? parameters, CancellationToken ct);
}
