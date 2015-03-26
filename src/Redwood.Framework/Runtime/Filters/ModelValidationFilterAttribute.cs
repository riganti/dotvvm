using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Hosting;
using Redwood.Framework.Validation;

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
            if (context.ModelState.ValidationTargetPath != null)
            {
                var path = Parser.Translation.ExpressionTranslator.CombinePaths(context.ModelState.DataContextPath, context.ModelState.ValidationTargetPath);
                path = path.Skip(path.FirstOrDefault() == "$root" ? 1 : 0).ToArray();

                // perform the validation
                context.ModelState.Errors.AddRange(viewModelValidator.ValidateViewModel(context.ViewModel, actionInfo.MethodInfo, path));

                // return the model state when error occurs
                context.FailOnInvalidModelState();
            }

            base.OnCommandExecuting(context, actionInfo);
        }
    }
}
