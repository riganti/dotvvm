#nullable enable
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Filters
{
    public interface IViewModelActionFilter : IActionFilter
    {
        /// <summary>
        /// Called after the viewmodel object is created.
        /// </summary>
        Task OnViewModelCreatedAsync(IDotvvmRequestContext context);

        /// <summary>
        /// Called after the viewmodel is deserialized on postback.
        /// </summary>
        Task OnViewModelDeserializedAsync(IDotvvmRequestContext context);

        /// <summary>
        /// Called before the viewmodel is serialized.
        /// </summary>
        Task OnViewModelSerializingAsync(IDotvvmRequestContext context);

    }
}
