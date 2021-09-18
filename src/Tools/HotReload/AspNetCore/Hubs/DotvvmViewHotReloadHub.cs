using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;

namespace DotVVM.Diagnostics.ViewHotReload.AspNetCore.Hubs
{
    public class DotvvmViewHotReloadHub : Hub
    {
        internal static void NotifyFileChanged(IHubContext<DotvvmViewHotReloadHub> context, IEnumerable<string> virtualPaths)
        {
            context.Clients.All.SendAsync("fileChanged", virtualPaths);
        }
    }
}