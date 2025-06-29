using System.Reflection;
using DotVVM.Framework.Routing;

namespace DotVVM.Sitemap.Providers;

public class AppDeploymentTimeLastModificationDateProvider : IRouteLastModificationDateProvider
{
    private static readonly DateTime appDeploymentTime;

    static AppDeploymentTimeLastModificationDateProvider()
    {
        var entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
        appDeploymentTime = entryAssemblyLocation != null
            ? File.GetLastWriteTime(entryAssemblyLocation)
            : throw new NotSupportedException($"{nameof(AppDeploymentTimeLastModificationDateProvider)} cannot be used in environments without the entry assembly.");
    }

    public Task<DateTime> GetLastModifiedTimeAsync(RouteBase route, string? culture, IDictionary<string, object?>? parameters, CancellationToken ct)
    {
        return Task.FromResult(appDeploymentTime);
    }
}
