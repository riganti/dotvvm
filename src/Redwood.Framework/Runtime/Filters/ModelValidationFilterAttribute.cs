using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Hosting;
using Redwood.Framework.ViewModel;

namespace Redwood.Framework.Runtime.Filters
{

    /// <summary>
    /// Runs the model validation and returns the errors if the viewModel is not valid.
    /// </summary>
    public class ModelValidationFilterAttribute : ActionFilterAttribute
    {

        private ViewModelValidator viewModelValidator = new ViewModelValidator();


        /// <summary>
        /// Called before the command is executed.
        /// </summary>
        protected internal override void OnCommandExecuting(RedwoodRequestContext context, ActionInfo actionInfo)
        {
            if (!string.IsNullOrEmpty(context.ModelState.ValidationTargetPath))
            {
                // perform the validation
                context.ModelState.Errors.AddRange(viewModelValidator.ValidateViewModel(context.ModelState.ValidationTarget));

                // return the model state when error occurs
                if (!context.ModelState.IsValid)
                {
                    context.OwinContext.Response.ContentType = "application/json";
                    context.OwinContext.Response.Write(context.Presenter.ViewModelSerializer.SerializeModelState(context));
                    throw new RedwoodInterruptRequestExecutionException("The ViewModel contains validation errors!");
                }
            }
            
            base.OnCommandExecuting(context, actionInfo);
        }
    }
}
