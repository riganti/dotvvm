using DotVVM.Framework.Hosting;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace DotVVM.Framework.Diagnostics.StatusPage
{
    public class StatusPageOptions
    {
        public string RouteName { get; set; } = "StatusPage";

        public string Url { get; set; } = "_dotvvm/status";

        public bool CompileAfterPageLoads { get; set; } = true;

        public Func<IDotvvmRequestContext, Task<bool>> Authorize { get; set; }
            = context => Task.FromResult(context.HttpContext.Request.Url.IsLoopback);

        public static StatusPageOptions CreateDefault()
        {
            return new StatusPageOptions();
        }
    }
}
