using DotVVM.HotReload;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.HotReload.AspNetCore.Services;

namespace DotVVM.Framework.Configuration
{
    public static class DotvvmServiceCollectionExtensions
    {

        public static void AddHotReload(this IDotvvmServiceCollection services)
        {
            services.Services.AddSignalR();

            services.Services.AddSingleton<IMarkupFileChangeNotifier, AspNetCoreMarkupFileChangeNotifier>();
            services.Services.AddSingleton<IMarkupFileLoader, HotReloadAggregateMarkupFileLoader>();

            services.Services.Configure<DotvvmConfiguration>(RegisterResources);
            services.Services.AddTransient<ResourceManager>(provider =>
            {
                var manager = new ResourceManager(provider.GetRequiredService<DotvvmResourceRepository>());
                manager.AddRequiredResource("dotvvm-hotreload");
                return manager;
            });
        }

        private static void RegisterResources(DotvvmConfiguration config)
        {
            if (config.Resources.FindResource("signalr") == null)
            {
                config.Resources.Register("signalr", new ScriptResource(new EmbeddedResourceLocation(typeof(DotvvmServiceCollectionExtensions).Assembly, "DotVVM.HotReload.AspNetCore.Scripts.signalr.min.js")));
            }

            config.Resources.Register("dotvvm-hotreload", new ScriptResource(new EmbeddedResourceLocation(typeof(DotvvmServiceCollectionExtensions).Assembly, "DotVVM.HotReload.AspNetCore.Scripts.dotvvm.hotreload.js"))
            {
                Dependencies = new[] { "signalr", "dotvvm" }
            });
        }
    }
}
