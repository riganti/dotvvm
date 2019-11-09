#nullable enable
using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime.Filters
{
    /// <summary>
    /// Allows to modify the response when an exception occurs.
    /// </summary>
    public abstract class ExceptionFilterAttribute : ActionFilterAttribute
    {
        protected internal override Task OnCommandExecutedAsync(IDotvvmRequestContext context, ActionInfo actionInfo, Exception? exception)
        {
            if (exception != null)
            {
                return OnCommandExceptionAsync(context, actionInfo, exception);
            }
            else
            {
                return TaskUtils.GetCompletedTask();
            }
        }

        /// <summary>
        /// Called when the exception occurs during the command invocation.
        /// </summary>
        protected virtual Task OnCommandExceptionAsync(IDotvvmRequestContext context, ActionInfo actionInfo, Exception ex)
            => TaskUtils.GetCompletedTask();
    }
}
