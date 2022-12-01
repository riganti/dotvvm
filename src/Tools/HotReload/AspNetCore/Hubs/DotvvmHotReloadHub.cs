using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;

namespace DotVVM.HotReload.AspNetCore.Hubs
{
    public class DotvvmHotReloadHub : Hub
    {
        internal static void NotifyFileChanged(IHubContext<DotvvmHotReloadHub> context, IEnumerable<string> virtualPaths)
        {
            context.Clients.All.SendAsync("fileChanged", virtualPaths);
        }
    }
}
