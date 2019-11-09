#nullable enable
using System;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime.Filters
{
    public interface IPresenterActionFilter : IActionFilter
    {

        /// <summary>
        /// Called before presenter starts processing HTTP request.
        /// </summary>
        Task OnPresenterExecutingAsync(IDotvvmRequestContext context);

        /// <summary>
        /// Called after presenter completes processing HTTP request.
        /// </summary>
        Task OnPresenterExecutedAsync(IDotvvmRequestContext context);

        /// <summary>
        /// Called when an exception occurs during the processing of the request.
        /// </summary>
        Task OnPresenterExceptionAsync(IDotvvmRequestContext context, Exception exception);
    }
}
