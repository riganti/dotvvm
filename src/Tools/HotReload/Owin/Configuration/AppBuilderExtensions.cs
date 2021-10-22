using Owin;
using Microsoft.Owin;
using System;
using Microsoft.AspNet.SignalR;

namespace Microsoft.Owin
{
    public static class AppBuilderExtensions
    {

        public static void UseDotvvmHotReload(this IAppBuilder app)
        {
            app.MapSignalR();
        }

    }
}
