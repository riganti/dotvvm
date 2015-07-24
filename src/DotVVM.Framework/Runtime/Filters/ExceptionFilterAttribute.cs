using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime.Filters
{
    /// <summary>
    /// Allows to modify the response when an exception occurs.
    /// </summary>
    public abstract class ExceptionFilterAttribute : ActionFilterAttribute
    {

        /// <summary>
        /// Called after the command is invoked.
        /// </summary>
        protected internal override void OnCommandExecuted(DotvvmRequestContext context, ActionInfo actionInfo, Exception exception)
        {
            if (exception != null)
            {
                OnException(context, actionInfo, exception);
            }
        }

        /// <summary>
        /// Called when the exception occurs during the command invocation.
        /// </summary>
        protected virtual void OnException(DotvvmRequestContext context, ActionInfo actionInfo, Exception ex)
        {
        }

    }
}