using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime.Filters
{
    /// <summary>
    /// Allows to add custom logic before and after a command is executed on a ViewModel.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public abstract class ActionFilterAttribute : Attribute
    {
        /// <summary>
        /// Called after the viewmodel object is created.
        /// </summary>
        protected internal virtual Task OnViewModelCreatedAsync(IDotvvmRequestContext context)
            => Task.CompletedTask;

        /// <summary>
        /// Called before the command is executed.
        /// </summary>
        protected internal virtual Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo)
            => Task.CompletedTask;

        /// <summary>
        /// Called after the command is executed.
        /// </summary>
        protected internal virtual Task OnCommandExecutedAsync(IDotvvmRequestContext context, ActionInfo actionInfo, Exception exception)
            => Task.CompletedTask;

        /// <summary>
        /// Called before the response is rendered.
        /// </summary>
        protected internal virtual Task OnResponseRenderingAsync(IDotvvmRequestContext context)
            => Task.CompletedTask;

        /// <summary>
        /// Called when an exception occurs during the processing of the page events.
        /// </summary>
        protected internal virtual Task OnPageExceptionAsync(IDotvvmRequestContext context, Exception exception)
            => Task.CompletedTask;
    }
}