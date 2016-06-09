using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Framework.Runtime.Filters
{

    /// <summary>
    /// Runs the model validation and returns the errors if the viewModel is not valid.
    /// </summary>
    public class ModelValidationFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Called before the command is executed.
        /// </summary>
        protected internal override void OnCommandExecuting(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            if (!string.IsNullOrEmpty(context.ModelState.ValidationTargetPath))
            {
                var viewModelValidator = context.Configuration.ServiceLocator.GetService<IViewModelValidator>();

                // perform the validation
                context.ModelState.Errors.AddRange(viewModelValidator.ValidateViewModel(context.ModelState.ValidationTarget));

                // return the model state when error occurs
                context.FailOnInvalidModelState();
            }

            base.OnCommandExecuting(context, actionInfo);
        }
    }
}
