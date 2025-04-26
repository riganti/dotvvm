using DotVVM.Framework.Routing;
using DotVVM.Sitemap.Providers;

namespace DotVVM.Sitemap.Options;

public class RouteSitemapOptions
{
    public double Priority { get; set; } = 1;
    public ChangeFrequency ChangeFrequency { get; set; } = ChangeFrequency.Always;
    public DateTime? LastModified { get; set; } = null;
    public Type LastModifiedProviderType { get; private set; } = typeof(AppDeploymentTimeLastModificationDateProvider);
    public Type? ParameterValuesProviderType { get; private set; } = null;

    public RouteSitemapOptions UseLastModifiedProvider<T>() where T : IRouteLastModificationDateProvider
    {
        LastModifiedProviderType = typeof(T);
        return this;
    }

    public RouteSitemapOptions UseParameterValuesProvider<T>() where T : IRouteParameterValuesProvider
    {
        ParameterValuesProviderType = typeof(T);
        return this;
    }
}
