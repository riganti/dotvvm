using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Binding;
using StackExchange.Profiling;
using Microsoft.Owin;


namespace DotVVM.Tracing.MiniProfiler.Owin
{
    public class MiniProfilerActionFilter : ActionFilterAttribute
    {
        private readonly PathString resultsLink;

        public MiniProfilerActionFilter()
        {
            var basePath = new PathString(
                StackExchange.Profiling.MiniProfiler.Settings.RouteBasePath.Replace("~/", "/"));

            resultsLink = basePath.Add(new PathString("/results-index"));
        }

        protected override Task OnPageLoadedAsync(IDotvvmRequestContext context)
        {
            // Naming for PostBack occurs in method OnCommandExecutingAsync
            // We don't want to override it with less information
            if (!context.IsPostBack)
            {
                AddMiniProfilerName(context, context.HttpContext.Request.Url.AbsoluteUri);
            }

            return base.OnPageLoadedAsync(context);
        }

        protected override Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            var commandCode = actionInfo.Binding
                ?.GetProperty<OriginalStringBindingProperty>(Framework.Binding.Expressions.ErrorHandlingMode.ReturnNull)?.Code;

            AddMiniProfilerName(context, context.HttpContext.Request.Url.AbsoluteUri,
                $"(PostBack {commandCode})");

            return base.OnCommandExecutingAsync(context, actionInfo);
        }

        private void AddMiniProfilerName(IDotvvmRequestContext context, params string[] nameParts)
        {
            var currentMiniProfiler = StackExchange.Profiling.MiniProfiler.Current;

            if (currentMiniProfiler != null)
            {
                currentMiniProfiler.AddCustomLink("results", resultsLink.ToString());
                currentMiniProfiler.Name = string.Join(" ", nameParts);
            }
        }
    }
}
