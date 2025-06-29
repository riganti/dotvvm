using DotVVM.Framework.Routing;
using DotVVM.Sitemap.Providers;

namespace DotVVM.Sitemap.Options;

public class RouteSitemapOptions
{
    public double? Priority { get; set; }
    public ChangeFrequency? ChangeFrequency { get; set; }
    public DateTime? LastModified { get; set; }
    public Type? LastModifiedProviderType { get; private set; }
    public Type? ParameterValuesProviderType { get; private set; }
    public bool? Exclude { get; set; }

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

    public RouteSitemapOptions CreateDerivedOptions(RouteSitemapOptions options)
    {
        return new RouteSitemapOptions()
        {
            Priority = options.Priority ?? Priority,
            ChangeFrequency = options.ChangeFrequency ?? ChangeFrequency,
            LastModified = options.LastModified ?? LastModified,
            LastModifiedProviderType = options.LastModifiedProviderType ?? LastModifiedProviderType,
            ParameterValuesProviderType = options.ParameterValuesProviderType ?? ParameterValuesProviderType,
            Exclude = options.Exclude ?? Exclude
        };
    }
}
