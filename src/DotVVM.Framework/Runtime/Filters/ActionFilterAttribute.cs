using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime.Filters
{
    /// <summary>
    /// Allows to add custom logic into different phases of a request processing, e.g.: before and after a command is executed on a ViewModel.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ActionFilterAttribute : Attribute, IPageActionFilter, ICommandActionFilter, IViewModelActionFilter, IPresenterActionFilter
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
        /// Called before the viewmodel is serialized.
        /// </summary>
        protected internal virtual Task OnViewModelSerializingAsync(IDotvvmRequestContext context)
            => TaskUtils.GetCompletedTask();

        /// <summary>
        /// Called after the viewmodel is deserialized on postback.
        /// </summary>
        protected internal virtual Task OnViewModelDeserializedAsync(IDotvvmRequestContext context)
            => TaskUtils.GetCompletedTask();

        /// <summary>
        /// Called when an exception occurs during the processing of the page events.
        /// </summary>
        protected internal virtual Task OnPageExceptionAsync(IDotvvmRequestContext context, Exception exception)
            => TaskUtils.GetCompletedTask();

        /// <inheritdoc cref="IPresenterActionFilter.OnPresenterExecutingAsync"/>
        protected internal virtual Task OnPresenterExecutingAsync(IDotvvmRequestContext context)
            => TaskUtils.GetCompletedTask();

        /// <inheritdoc cref="IPresenterActionFilter.OnPresenterExecutedAsync"/>
        protected internal virtual Task OnPresenterExecutedAsync(IDotvvmRequestContext context)
            => TaskUtils.GetCompletedTask();

        /// <inheritdoc cref="IPresenterActionFilter.OnPresenterExceptionAsync"/>
        protected internal virtual Task OnPresenterExceptionAsync(IDotvvmRequestContext context, Exception exception)
            => TaskUtils.GetCompletedTask();

        /// <inheritdoc cref="IPageActionFilter.OnPageInitializedAsync"/>
        protected internal virtual Task OnPageInitializedAsync(IDotvvmRequestContext context)
            => TaskUtils.GetCompletedTask();

        /// <inheritdoc cref="IPageActionFilter.OnPageRenderedAsync"/>
        protected internal virtual Task OnPageRenderedAsync(IDotvvmRequestContext context)
            => TaskUtils.GetCompletedTask();

        Task IPresenterActionFilter.OnPresenterExceptionAsync(IDotvvmRequestContext context, Exception exception) => OnPresenterExceptionAsync(context, exception);
        Task IPresenterActionFilter.OnPresenterExecutingAsync(IDotvvmRequestContext context) => OnPresenterExecutingAsync(context);
        Task IPresenterActionFilter.OnPresenterExecutedAsync(IDotvvmRequestContext context) => OnPresenterExecutedAsync(context);

        Task IPageActionFilter.OnPageExceptionAsync(IDotvvmRequestContext context, Exception exception) => OnPageExceptionAsync(context, exception);
        Task ICommandActionFilter.OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo) => OnCommandExecutingAsync(context, actionInfo);
        Task ICommandActionFilter.OnCommandExecutedAsync(IDotvvmRequestContext context, ActionInfo actionInfo, Exception exception) => OnCommandExecutedAsync(context, actionInfo, exception);
        Task IViewModelActionFilter.OnViewModelCreatedAsync(IDotvvmRequestContext context) => OnViewModelCreatedAsync(context);
        Task IViewModelActionFilter.OnViewModelDeserializedAsync(IDotvvmRequestContext context) => OnViewModelDeserializedAsync(context);
        Task IViewModelActionFilter.OnViewModelSerializingAsync(IDotvvmRequestContext context) => OnViewModelSerializingAsync(context);
        Task IPageActionFilter.OnPageInitializedAsync(IDotvvmRequestContext context) => OnPageInitializedAsync(context);
        Task IPageActionFilter.OnPageRenderedAsync(IDotvvmRequestContext context) => OnPageRenderedAsync(context);
    }
}
