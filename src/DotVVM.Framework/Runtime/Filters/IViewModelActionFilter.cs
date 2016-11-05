using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Filters
{
    public interface IViewModelActionFilter
    {
        /// <summary>
        /// Called after the viewmodel object is created.
        /// </summary>
        Task OnViewModelCreatedAsync(IDotvvmRequestContext context);
        /// <summary>
        /// Called before the response is rendered.
        /// </summary>
        Task OnResponseRenderingAsync(IDotvvmRequestContext context);
    }
}
