using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Binding;
using StackExchange.Profiling;
using Microsoft.Extensions.Options;

namespace DotVVM.Tracing.MiniProfiler
{
    public class MiniProfilerActionFilter : ActionFilterAttribute
    {
        public MiniProfilerActionFilter()
        {
        }

        protected override Task OnPresenterExecutingAsync(IDotvvmRequestContext context)
        {
            // Naming for PostBack occurs in method OnCommandExecutingAsync
            // We don't want to override it with less information
            if (!context.IsPostBack)
            {
                AddMiniProfilerName(context, context.HttpContext.Request.Url.AbsoluteUri);
            }

            return base.OnPresenterExecutingAsync(context);
        }

        protected override Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            var commandCode = actionInfo.Binding
                ?.GetProperty<OriginalStringBindingProperty>(Framework.Binding.Expressions.ErrorHandlingMode.ReturnNull)?.Code;

            var postbackSuffix = commandCode != null ? $"({(actionInfo.IsControlCommand ? "Control Command:" : "Command:")} {commandCode})" : "(Static Command)";

            AddMiniProfilerName(context, context.HttpContext.Request.Url.AbsoluteUri, postbackSuffix);

            return base.OnCommandExecutingAsync(context, actionInfo);
        }

        private void AddMiniProfilerName(IDotvvmRequestContext context, params string[] nameParts)
        {
            var currentMiniProfiler = StackExchange.Profiling.MiniProfiler.Current;

            if (currentMiniProfiler != null)
            {
                currentMiniProfiler.Name = string.Join(" ", nameParts);
            }
        }
    }
}