#nullable enable
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Filters
{
    public interface ICommandActionFilter : IActionFilter
    {
        /// <summary>
        /// Called before the command is executed.
        /// </summary>
        Task OnCommandExecutingAsync(IDotvvmRequestContext context, ActionInfo actionInfo);

        /// <summary>
        /// Called after the command is executed.
        /// </summary>
        Task OnCommandExecutedAsync(IDotvvmRequestContext context, ActionInfo actionInfo, Exception? exception);
    }
}
