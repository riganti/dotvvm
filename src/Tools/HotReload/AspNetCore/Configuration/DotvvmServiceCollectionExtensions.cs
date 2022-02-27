using DotVVM.HotReload;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting.ErrorPages;
using DotVVM.HotReload.AspNetCore.Services;

namespace DotVVM.Framework.Configuration
{
    public static class DotvvmServiceCollectionExtensions
    {

        public static void AddHotReload(this IDotvvmServiceCollection services)
        {
            services.Services.AddSignalR();

            services.Services.AddSingleton<IMarkupFileChangeNotifier, AspNetCoreMarkupFileChangeNotifier>();
            services.Services.Configure<AggregateMarkupFileLoaderOptions>(options =>
            {
                var index = options.LoaderTypes.FindIndex(l => l == typeof(DefaultMarkupFileLoader));
                if (index < 0)
                {
                    throw new InvalidOperationException("DotVVM Hot reload could not be initialized - the DefaultMarkupLoader was not found in the AggregateMarkupFileLoader Loaders collection.");
                }

                options.LoaderTypes[index] = typeof(HotReloadMarkupFileLoader);
            });
            services.Services.AddSingleton<HotReloadMarkupFileLoader>();

            services.Services.Configure<DotvvmConfiguration>(RegisterResources);
            services.Services.AddTransient<ResourceManager>(provider =>
            {
                var manager = new ResourceManager(provider.GetRequiredService<DotvvmResourceRepository>());
                var config = provider.GetRequiredService<DotvvmConfiguration>();
                if (config.Debug)
                {
                    manager.AddRequiredResource("dotvvm-hotreload");
                }

                return manager;
            });

            services.Services.AddSingleton<IErrorPageExtension, HotReloadErrorPageExtension>();
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
