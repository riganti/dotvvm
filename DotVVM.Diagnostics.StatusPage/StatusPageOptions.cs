using DotVVM.Framework.Hosting;
using System;
using System.Threading.Tasks;

namespace DotVVM.Diagnostics.StatusPage
{
    public class StatusPageOptions
    {
        public string RouteName { get; set; } = "StatusPage";

        public string Url { get; set; } = "_diagnostics/status";

        public Func<IDotvvmRequestContext, Task<bool>> Authorize { get; set; }
            = context => Task.FromResult(context.HttpContext.Request.Url.IsLoopback);

        public static StatusPageOptions CreateDefaultOptions()
        {
            return new StatusPageOptions();
        }
    }
}
