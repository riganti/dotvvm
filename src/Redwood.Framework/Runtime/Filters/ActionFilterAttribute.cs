using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.Runtime.Filters
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
        protected internal virtual void OnViewModelCreated(RedwoodRequestContext context)
        {
        }


        /// <summary>
        /// Called before the command is executed.
        /// </summary>
        protected internal virtual void OnCommandExecuting(RedwoodRequestContext context, ActionInfo actionInfo)
        {
        }


        /// <summary>
        /// Called after the command is executed.
        /// </summary>
        protected internal virtual void OnCommandExecuted(RedwoodRequestContext context, ActionInfo actionInfo, Exception exception)
        {
        }

        /// <summary>
        /// Called before the response is rendered.
        /// </summary>
        protected internal virtual void OnResponseRendering(RedwoodRequestContext context)
        {
        }

    }
}
