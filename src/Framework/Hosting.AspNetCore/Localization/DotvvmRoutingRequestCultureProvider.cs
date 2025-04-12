#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Hosting.AspNetCore.Localization;

public class DotvvmRoutingRequestCultureProvider : IRequestCultureProvider
{
    private static readonly object locker = new();
    private static IReadOnlyList<LocalizedDotvvmRoute>? cachedRoutes;

    public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        EnsureCachedRoutes(httpContext);

        // find matching localizable route and extract culture from it
        var url = DotvvmRoutingMiddleware.GetRouteMatchUrl(httpContext.Request.Path.Value!, httpContext.Request.QueryString.Value!);
        foreach (var route in cachedRoutes!)
        {
            if (route.IsPartialMatch(url, out _, out var values, out var matchedCulture))
            {
                return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(matchedCulture));
            }
        }

        return Task.FromResult<ProviderCultureResult?>(null);
    }

    private void EnsureCachedRoutes(HttpContext httpContext)
    {
        if (cachedRoutes == null)
        {
            lock (locker)
            {
                if (cachedRoutes == null)
                {
                    // try to obtain DotVVM configuration
                    if (httpContext.RequestServices.GetService<DotvvmConfiguration>() is not { } config)
                    {
                        throw new InvalidOperationException("DotVVM configuration not found in the service provider.");
                    }

                    // try to obtain DotVVM routes
                    cachedRoutes = config.RouteTable
                        .OfType<LocalizedDotvvmRoute>()
                        .ToArray();
                }
            }
        }
    }
}
