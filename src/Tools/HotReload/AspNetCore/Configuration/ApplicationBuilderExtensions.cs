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
            app.UseEndpoints(e =>
            {
                e.MapHub<DotvvmHotReloadHub>("/_dotvvm/hotReloadHub");
            });
        }

        public static void MapDotvvmHotReload(this IEndpointRouteBuilder app)
        {
            app.MapHub<DotvvmHotReloadHub>("/_dotvvm/hotReloadHub");
        }

    }
}
