using System.Collections.Generic;
using DotVVM.HotReload.AspNetCore.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DotVVM.HotReload.AspNetCore.Services
{
    public class AspNetCoreMarkupFileChangeNotifier : IMarkupFileChangeNotifier
    {
        private readonly IHubContext<DotvvmHotReloadHub> hubContext;

        public AspNetCoreMarkupFileChangeNotifier(IHubContext<DotvvmHotReloadHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public void NotifyFileChanged(IEnumerable<string> virtualPaths)
        {
            DotvvmHotReloadHub.NotifyFileChanged(hubContext, virtualPaths);
        }
    }
}
