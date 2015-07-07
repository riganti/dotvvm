using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Binding
{
    /// <summary>
    /// Performs the event validation.
    /// </summary>
    public class EventValidator
    {

        /// <summary>
        /// Validates the command.
        /// </summary>
        public void ValidateCommand(string[] path, string command, DotvvmControl viewRootControl, ref string validationTargetPath)
        {
            // find the binding
            DotvvmProperty targetProperty;
            DotvvmBindableControl targetControl;
            var binding = FindCommandBinding(path, command, viewRootControl, ref validationTargetPath, out targetControl, out targetProperty);
            if (binding == null)
            {
                ThrowEventValidationException();
                return;
            }

            // validate the command against the control
            if (targetControl is IEventValidationHandler)
            {
                if (!((IEventValidationHandler)targetControl).ValidateCommand(targetProperty))
                {
                    ThrowEventValidationException();
                    return;
                }
            }
        }

        /// <summary>
        /// Finds the binding of the specified type on the specified viewmodel path.
        /// </summary>
        private CommandBindingExpression FindCommandBinding(string[] path, string command, DotvvmControl viewRootControl, ref string validationTargetPath, out DotvvmBindableControl targetControl, out DotvvmProperty targetProperty)
        {
            // walk the control tree and find the path
            CommandBindingExpression result = null;
            DotvvmBindableControl resultControl = null;
            DotvvmProperty resultProperty = null;
            string resultValidationTargetPath = validationTargetPath;

            var walker = new NonEvaluatingControlTreeWalker(viewRootControl);
            walker.ProcessControlTree((ViewModel, control) =>
            {
                // compare path
                if (result == null && control is DotvvmBindableControl && ViewModelPathComparer.AreEqual(path, walker.CurrentPathArray))
                {
                    // find bindings of current control
                    var bindableControl = (DotvvmBindableControl)control;
                    var binding = bindableControl.GetAllBindings().Where(p => p.Value is CommandBindingExpression)
                        .FirstOrDefault(b => b.Value.Expression == command);
                    if (binding.Key != null)
                    {
                        // we have found the binding, now get the validation path
                        var currentValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(bindableControl, true);
                        if (currentValidationTargetPath == resultValidationTargetPath)
                        {
                            // the validation path is equal, we have found the binding
                            result = (CommandBindingExpression)binding.Value;
                            resultControl = bindableControl;
                            resultProperty = binding.Key;
                            resultValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(bindableControl, false);
                        }
                    }
                }
            });

            validationTargetPath = resultValidationTargetPath;
            targetControl = resultControl;
            targetProperty = resultProperty;
            return result;
        }

        /// <summary>
        /// Validates the control command.
        /// </summary>
        public void ValidateControlCommand(string[] path, string command, DotvvmControl viewRootControl, DotvvmControl targetControl, ref string validationTargetPath)
        {
            // find the binding
            DotvvmProperty targetProperty;
            var binding = FindControlCommandBinding(path, command, viewRootControl, (DotvvmBindableControl)targetControl, ref validationTargetPath, out targetProperty);
            if (binding == null)
            {
                ThrowEventValidationException();
                return;
            }
        }

        /// <summary>
        /// Finds the binding of the specified type on the specified viewmodel path.
        /// </summary>
        private ControlCommandBindingExpression FindControlCommandBinding(string[] path, string command, DotvvmControl viewRootControl, DotvvmBindableControl targetControl, ref string validationTargetPath, out DotvvmProperty targetProperty)
        {
            // walk the control tree and find the path
            ControlCommandBindingExpression result = null;
            DotvvmProperty resultProperty = null;
            string resultValidationTargetPath = validationTargetPath;

            var walker = new NonEvaluatingControlTreeWalker(viewRootControl);
            walker.ProcessControlTree((ViewModel, control) =>
            {
                // compare path
                if (control is DotvvmBindableControl && ViewModelPathComparer.AreEqual(path, walker.CurrentPathArray))
                {
                    // find bindings of current control
                    var bindableControl = (DotvvmBindableControl)control;
                    var binding = bindableControl.GetAllBindings().Where(p => p.Value is ControlCommandBindingExpression)
                        .FirstOrDefault(b => b.Value.Expression == command);
                    if (binding.Key != null)
                    {
                        // verify that the target control is the control command target
                        if (bindableControl.GetClosestControlBindingTarget() == targetControl)
                        {
                            // we have found the binding, now get the validation path
                            var currentValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(bindableControl, true);
                            if (currentValidationTargetPath == resultValidationTargetPath)
                            {
                                // the validation path is equal, we have found the binding
                                result = (ControlCommandBindingExpression)binding.Value;
                                resultProperty = binding.Key;
                                resultValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(bindableControl, false);
                            }
                        }
                    }
                }
            });

            validationTargetPath = resultValidationTargetPath;
            targetProperty = resultProperty;
            return result;
        }

        /// <summary>
        /// Throws the event validation exception.
        /// </summary>
        private void ThrowEventValidationException()
        {
            throw new Exception("Illegal command invocation!");
        }
    }
}