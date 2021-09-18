using Owin;
using Microsoft.Owin;
using System;
using Microsoft.AspNet.SignalR;

namespace Microsoft.Owin
{
    public static class AppBuilderExtensions
    {

        public static void UseDotvvmViewHotReload(this IAppBuilder app)
        {
            app.MapSignalR();
        }

    }
}
