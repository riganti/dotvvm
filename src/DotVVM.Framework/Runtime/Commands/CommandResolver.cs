using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.Runtime.Commands
{
    /// <summary>
    /// Finds the command to execute in the viewmodel using the path and command expression.
    /// </summary>
    public class CommandResolver
    {
        private EventValidator eventValidator = new EventValidator();

        /// <summary>
        /// Resolves the command called on the DotvvmControl.
        /// </summary>
        public ActionInfo GetFunction(DotvvmControl targetControl, DotvvmControl viewRootControl, DotvvmRequestContext context, string[] path, string commandId)
        {
            // event validation
            var validationTargetPath = context.ModelState.ValidationTargetPath;
            FindBindingResult findResult = null;
            if (targetControl == null)
            {
                findResult = eventValidator.ValidateCommand(path, commandId, viewRootControl, validationTargetPath);
            }
            else
            {
                findResult = eventValidator.ValidateControlCommand(path, commandId, viewRootControl, targetControl, validationTargetPath);
            }

            context.ModelState.ValidationTarget = findResult.Control.GetValue(Controls.Validation.TargetProperty) ?? context.ViewModel;

            return new ActionInfo
            {
                Action = () => findResult.Binding.Evaluate(findResult.Control, findResult.Property),
                Binding = findResult.Binding,
                IsControlCommand = targetControl != null
            };
        }

        /// <summary>
        /// Resolves the command called on the ViewModel.
        /// </summary>
        public ActionInfo GetFunction(DotvvmControl viewRootControl, DotvvmRequestContext context, string[] path, string command)
        {
            return GetFunction(null, viewRootControl, context, path, command);
        }
    }
}