using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime.Filters
{
    /// <summary>
    /// Allows to add custom logic before and after a command is executed on a ViewModel.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ActionFilterAttribute : Attribute, IRequestActionFilter, ICommandActionFilter, IViewModelActionFilter
    {
        /// <summary>
        /// Called after the viewmodel object is created.
        /// </summary>
        protected internal virtual Task OnViewModelCreatedAsync(IDotvvmRequestContext context)
            => TaskUtils.GetCompletedTask();

        /// <summary>
        /// Called before the command is executed.
        /// </summary>
        protected internal virtual Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo)
            => TaskUtils.GetCompletedTask();

        /// <summary>
        /// Called after the command is executed.
        /// </summary>
        protected internal virtual Task OnCommandExecutedAsync(IDotvvmRequestContext context, ActionInfo actionInfo, Exception exception)
            => TaskUtils.GetCompletedTask();

        /// <summary>
        /// Called before the response is rendered.
        /// </summary>
        protected internal virtual Task OnResponseRenderingAsync(IDotvvmRequestContext context)
            => TaskUtils.GetCompletedTask();

        /// <summary>
        /// Called when an exception occurs during the processing of the page events.
        /// </summary>
        protected internal virtual Task OnPageExceptionAsync(IDotvvmRequestContext context, Exception exception)
            => TaskUtils.GetCompletedTask();

        /// <summary>
        /// Called before page is processed.
        /// </summary>
        protected internal virtual Task OnPageLoadingAsync(IDotvvmRequestContext context)
            => TaskUtils.GetCompletedTask();

        /// <summary>
        /// Called after page is processed and ready to be sent to client.
        /// </summary>
        protected internal virtual Task OnPageLoadedAsync(IDotvvmRequestContext context)
            => TaskUtils.GetCompletedTask();

        Task IRequestActionFilter.OnPageExceptionAsync(IDotvvmRequestContext context, Exception exception) => OnPageExceptionAsync(context, exception);
        Task ICommandActionFilter.OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo) => OnCommandExecutingAsync(context, actionInfo);
        Task ICommandActionFilter.OnCommandExecutedAsync(IDotvvmRequestContext context, ActionInfo actionInfo, Exception exception) => OnCommandExecutedAsync(context, actionInfo, exception);
        Task IViewModelActionFilter.OnViewModelCreatedAsync(IDotvvmRequestContext context) => OnViewModelCreatedAsync(context);
        Task IViewModelActionFilter.OnResponseRenderingAsync(IDotvvmRequestContext context) => OnResponseRenderingAsync(context);
        Task IRequestActionFilter.OnPageLoadingAsync(IDotvvmRequestContext context) => OnPageLoadingAsync(context);
        Task IRequestActionFilter.OnPageLoadedAsync(IDotvvmRequestContext context) => OnPageLoadedAsync(context);
    }
}