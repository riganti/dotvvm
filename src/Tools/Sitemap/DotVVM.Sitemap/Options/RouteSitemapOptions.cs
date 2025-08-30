using System;

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
    /// Gets or sets whether the route should be excluded from the sitemap.
    /// </summary>
    public bool? Exclude { get; set; }

    public RouteSitemapOptions CreateDerivedOptions(RouteSitemapOptions? options)
    {
        return new RouteSitemapOptions()
        {
            Priority = options?.Priority ?? Priority,
            ChangeFrequency = options?.ChangeFrequency ?? ChangeFrequency,
            LastModified = options?.LastModified ?? LastModified,
            Exclude = options?.Exclude ?? Exclude
        };
    }
}
