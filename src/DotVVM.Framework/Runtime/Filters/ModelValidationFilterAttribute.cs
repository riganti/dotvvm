using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Runtime.Filters
{

    /// <summary>
    /// Runs the model validation and returns the errors if the viewModel is not valid.
    /// </summary>
    public class ModelValidationFilterAttribute : ActionFilterAttribute
    {

        private ViewModelValidator viewModelValidator = new ViewModelValidator();

        public string[] Groups { get; }

        public ModelValidationFilterAttribute(params string[] groups)
        {
            Groups = groups;
        }


        /// <summary>
        /// Called before the command is executed.
        /// </summary>
        protected internal override void OnCommandExecuting(IDotvvmRequestContext context, ActionInfo actionInfo)
        {
            //if (Groups != null) context.ModelState.ValidationGroups = new HashSet<string>(Groups);

            if (!string.IsNullOrEmpty(context.ModelState.ValidationTargetPath))
            {
                // perform the validation
                context.ModelState.Errors.AddRange(viewModelValidator.ValidateViewModel(context.ModelState.ValidationTarget, context.ModelState.ValidationGroups));

                // return the model state when error occurs
                context.FailOnInvalidModelState();
            }

            base.OnCommandExecuting(context, actionInfo);
        }
    }
}
