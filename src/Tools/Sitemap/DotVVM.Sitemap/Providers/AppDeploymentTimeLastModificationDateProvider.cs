using System.Reflection;
using DotVVM.Framework.Routing;

namespace DotVVM.Sitemap.Providers;

/// <summary>
/// Resolves the last modification date for a route based on the deployment time of the application's entry assembly.
/// </summary>
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

    /// <inheritdoc />
    public async Task<DateTime?> GetLastModifiedTimeAsync(RouteBase route, CancellationToken ct)
    {
        return appDeploymentTime;
    }
}
