using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;

namespace DotVVM.Diagnostics.ViewHotReload.Owin.Hubs
{
    public class DotvvmViewHotReloadHub : Hub
    {
        internal static void NotifyFileChanged(IHubContext context, IEnumerable<string> virtualPaths)
        {
            context.Clients.All.fileChanged(virtualPaths);
        }
    }
}