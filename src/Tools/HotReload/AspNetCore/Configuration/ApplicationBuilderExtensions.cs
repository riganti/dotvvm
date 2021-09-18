using DotVVM.Diagnostics.ViewHotReload.AspNetCore.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {

        public static void UseDotvvmViewHotReload(this IApplicationBuilder app)
        {
            app.UseSignalR(builder =>
            {
                builder.MapHub<DotvvmViewHotReloadHub>("/_diagnostics/dotvvmViewHotReloadHub");
            });
        }

        public static void MapDotvvmViewHotReload(this IEndpointRouteBuilder app)
        {
            app.MapHub<DotvvmViewHotReloadHub>("/_diagnostics/dotvvmViewHotReloadHub");
        }

    }
}
