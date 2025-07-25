using DotVVM.Framework.Routing;
using DotVVM.Sitemap.Providers;

namespace DotVVM.Sitemap.Options;

public class RouteSitemapOptions
{
    /// <summary>
    /// Gets or sets the priority of the route in the sitemap (should be between 0.0 to 1.0).
    /// </summary>
    public double? Priority { get; set; }

    /// <summary>
    /// Gets or sets the frequency at which the content is expected to change.
    /// </summary>
    public ChangeFrequency? ChangeFrequency { get; set; }

    /// <summary>
    /// Gets or sets the date of last modification of the content.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Gets or sets the type of the provider that resolves the last modification date for the route.
    /// </summary>
    public Type? LastModifiedProviderType { get; private set; }

    /// <summary>
    /// Gets or sets the type of the provider that resolves the parameter values for the route.
    /// If more routes use the same type of provider, the same instance will be used for all of them.
    /// </summary>
    public Type? ParameterValuesProviderType { get; private set; }

    /// <summary>
    /// Gets or sets whether the route should be excluded from the sitemap.
    /// </summary>
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
