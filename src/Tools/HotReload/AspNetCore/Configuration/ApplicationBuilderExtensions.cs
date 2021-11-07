using DotVVM.HotReload.AspNetCore.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {

        public static void UseDotvvmHotReload(this IApplicationBuilder app)
        {
            app.UseSignalR(builder =>
            {
                builder.MapHub<DotvvmHotReloadHub>("/_dotvvm/dotvvmHotReloadHub");
            });
        }

        public static void MapDotvvmHotReload(this IEndpointRouteBuilder app)
        {
            app.MapHub<DotvvmHotReloadHub>("/_dotvvm/dotvvmHotReloadHub");
        }

    }
}
