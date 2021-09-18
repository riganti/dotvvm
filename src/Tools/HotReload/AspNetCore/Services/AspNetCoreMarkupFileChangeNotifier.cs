using System.Collections.Generic;
using DotVVM.Diagnostics.ViewHotReload.AspNetCore.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DotVVM.Diagnostics.ViewHotReload.AspNetCore.Services
{
    public class AspNetCoreMarkupFileChangeNotifier : IMarkupFileChangeNotifier
    {
        private readonly IHubContext<DotvvmViewHotReloadHub> hubContext;

        public AspNetCoreMarkupFileChangeNotifier(IHubContext<DotvvmViewHotReloadHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public void NotifyFileChanged(IEnumerable<string> virtualPaths)
        {
            DotvvmViewHotReloadHub.NotifyFileChanged(hubContext, virtualPaths);
        }
    }
}