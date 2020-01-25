#nullable enable
using DotVVM.Framework.Binding;
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
        public ActionInfo GetFunction(DotvvmControl? targetControl, DotvvmControl viewRootControl, IDotvvmRequestContext context, string[] path, string commandId, object[] args)
        {
            // event validation
            var validationTargetPath = context.ModelState.ValidationTargetPath;
            var findResult = 
                targetControl == null ?
                eventValidator.ValidateCommand(path, commandId, viewRootControl, validationTargetPath) :
                eventValidator.ValidateControlCommand(path, commandId, viewRootControl, targetControl, validationTargetPath);

            context.ModelState.ValidationTarget = findResult.Control.GetValue(DotVVM.Framework.Controls.Validation.TargetProperty) ?? context.ViewModel;

            return new ActionInfo {
                Action = () => findResult.Binding.Evaluate(findResult.Control, args),
                Binding = findResult.Binding,
                IsControlCommand = targetControl != null
            };
        }

        /// <summary>
        /// Resolves the command called on the ViewModel.
        /// </summary>
        public ActionInfo GetFunction(DotvvmControl viewRootControl, IDotvvmRequestContext context, string[] path, string command, object[] args)
        {
            return GetFunction(null, viewRootControl, context, path, command, args);
        }
    }
}
