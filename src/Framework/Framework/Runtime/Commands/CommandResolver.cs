using System;
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
        public ActionInfo GetFunction(DotvvmControl? targetControl, DotvvmControl viewRootControl, IDotvvmRequestContext context, string[] path, string commandId, Func<Type, object>[] args)
        {
            // event validation
            var validationTargetPath = context.ModelState.ValidationTargetPath;
            var findResult = 
                targetControl == null ?
                eventValidator.ValidateCommand(path, commandId, viewRootControl, validationTargetPath) :
                eventValidator.ValidateControlCommand(path, commandId, viewRootControl, targetControl, validationTargetPath);

            context.ModelState.ValidationTarget = findResult.Control!.GetValue(Validation.TargetProperty);
            var validationEnabled = findResult.Control!.GetValue<bool>(Validation.EnabledProperty);

            if (context.ModelState.ValidationTarget == null && validationEnabled)
            {
                if (context.ModelState.ValidationTargetPath != KnockoutHelper.RootValidationTargetExpression)
                {
                    var property = context.ModelState.ValidationTargetPath;
                    throw new ArgumentException($"Validation target points to a property ({property}) that was evaluated to null");
                }

                context.ModelState.ValidationTarget = context.ViewModel;
            }

            return new ActionInfo(
                findResult.Binding,
                () => findResult.Binding!.Evaluate(findResult.Control, args),
                targetControl != null
            );
        }

        /// <summary>
        /// Resolves the command called on the ViewModel.
        /// </summary>
        public ActionInfo GetFunction(DotvvmControl viewRootControl, IDotvvmRequestContext context, string[] path, string command, Func<Type, object>[] args)
        {
            return GetFunction(null, viewRootControl, context, path, command, args);
        }
    }
}
