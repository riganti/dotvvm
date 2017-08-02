using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Tracing.MiniProfiler.Owin
{
    public class MiniProfilerActionFilter : ActionFilterAttribute
    {
        protected override Task OnPageLoadedAsync(IDotvvmRequestContext context)
        {
            var name = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path.Value}";
            AddMiniProfilerName(context, name);

            return base.OnPageLoadedAsync(context);
        }

        protected override Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            var name = $"POSTBACK {context.HttpContext.Request.Path.Value}";
            AddMiniProfilerName(context, name);

            return base.OnCommandExecutingAsync(context, actionInfo);
        }

        private void AddMiniProfilerName(IDotvvmRequestContext context, string name)
        {
            var currentMiniProfiler = StackExchange.Profiling.MiniProfiler.Current;

            if (currentMiniProfiler != null)
            {
                if (string.IsNullOrEmpty(currentMiniProfiler?.Name))
                {
                    currentMiniProfiler.Name = name;
                }
            }
        }
    }
}
