using DotVVM.HotReload.Owin.Hubs;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;

namespace DotVVM.HotReload.Owin.Services
{
    public class OwinMarkupFileChangeNotifier : IMarkupFileChangeNotifier
    {
        public void NotifyFileChanged(IEnumerable<string> virtualPaths)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<DotvvmHotReloadHub>();
            DotvvmHotReloadHub.NotifyFileChanged(context, virtualPaths);
        }
    }
}
