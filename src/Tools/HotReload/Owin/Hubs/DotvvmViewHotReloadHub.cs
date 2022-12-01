using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;

namespace DotVVM.HotReload.Owin.Hubs
{
    public class DotvvmHotReloadHub : Hub
    {
        internal static void NotifyFileChanged(IHubContext context, IEnumerable<string> virtualPaths)
        {
            context.Clients.All.fileChanged(virtualPaths);
        }
    }
}
