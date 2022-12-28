using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.AutoUI.Metadata;

namespace DotVVM.AutoUI.PropertyHandlers;

public static class AutoUIPropertyHandlerExtensions
{

    public static T? FindBestProvider<T>(this IEnumerable<T> providers, PropertyDisplayMetadata property, AutoUIContext context)
        where T : class, IAutoUIPropertyHandler
    {
        providers = providers.Where(p => p.CanHandleProperty(property, context));

        if (property.UIHints.Any())
        {
            if (providers.FirstOrDefault(p => property.UIHints.Any(h => p.UIHints.Contains(h, StringComparer.OrdinalIgnoreCase))) is { } matchingProvider)
            {
                return matchingProvider;
            }
        }

        return providers.FirstOrDefault();
    }

}
