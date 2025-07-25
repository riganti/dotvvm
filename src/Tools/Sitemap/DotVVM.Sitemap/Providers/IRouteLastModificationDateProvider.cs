using DotVVM.Framework.Routing;

namespace DotVVM.Sitemap.Providers;

public interface IRouteLastModificationDateProvider
{
    Task<DateTime?> GetLastModifiedTimeAsync(RouteBase route, CancellationToken ct);
}
