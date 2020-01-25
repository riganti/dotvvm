#nullable enable
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Filters
{
    public interface IPageActionFilter: IActionFilter
    {
        /// <summary>
        /// Called when an exception occurs during the processing of the page.
        /// </summary>
        Task OnPageExceptionAsync(IDotvvmRequestContext context, Exception exception);
        /// <summary>
        /// Called after page is initialized, just after the ViewModel instance is created
        /// </summary>
        Task OnPageInitializedAsync(IDotvvmRequestContext context);
        /// <summary>
        /// Called after page is rendered and ready to be sent to client.
        /// </summary>
        Task OnPageRenderedAsync(IDotvvmRequestContext context);
    }
}
