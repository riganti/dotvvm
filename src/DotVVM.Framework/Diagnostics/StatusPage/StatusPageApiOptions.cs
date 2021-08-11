using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Diagnostics.StatusPage
{
    public class StatusPageApiOptions : StatusPageOptions
    {
        public string RouteName { get; set; } = "StatusPageApi";

        public string Url { get; set; } = "_diagnostics/status/api";

        public Func<IDotvvmRequestContext, Task<bool>> Authorize { get; set; }
            = context => Task.FromResult(context.HttpContext.Request.Url.IsLoopback);

        public NonAuthorizedApiAccessMode NonAuthorizedApiAccessMode { get; set; } = NonAuthorizedApiAccessMode.Deny;

        public static StatusPageApiOptions CreateDefaultOptions()
        {
            return new StatusPageApiOptions();
        }
    }
}
