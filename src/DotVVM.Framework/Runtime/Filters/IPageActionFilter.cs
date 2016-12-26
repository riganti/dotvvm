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
        /// Called before page is processed.
        /// </summary>
        Task OnPageLoadingAsync(IDotvvmRequestContext context);
        /// <summary>
        /// Called after page is processed and ready to be sent to client.
        /// </summary>
        Task OnPageLoadedAsync(IDotvvmRequestContext context);
        /// <summary>
        /// Called when an exception occurs during the processing of the page.
        /// </summary>
        Task OnPageExceptionAsync(IDotvvmRequestContext context, Exception exception);
    }
}
