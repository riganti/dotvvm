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
        public FindBindingResult ValidateCommand(string[] path, string commandId, DotvvmControl viewRootControl, string validationTargetPath)
        {
            // find the binding
            var result = FindCommandBinding(path, commandId, viewRootControl, validationTargetPath);
            if (result == null || result.Binding == null)
            {
                throw EventValidationException();
            }

            // validate the command against the control
            if (result.Control is IEventValidationHandler)
            {
                if (!((IEventValidationHandler)result.Control).ValidateCommand(result.Property))
                {
                    throw EventValidationException();
                }
            }
            return result;
        }

        /// <summary>
        /// Finds the binding of the specified type on the specified viewmodel path.
        /// </summary>
        private FindBindingResult FindCommandBinding(string[] path, string commandId, DotvvmControl viewRootControl, string validationTargetPath)
        {
            // walk the control tree and find the path
            CommandBindingExpression resultBinding = null;
            DotvvmBindableControl resultControl = null;
            DotvvmProperty resultProperty = null;

            var walker = new ControlTreeWalker(viewRootControl);
            walker.ProcessControlTree((control) =>
            {
                // compare path
                if (resultBinding == null && control is DotvvmBindableControl && ViewModelPathComparer.AreEqual(path, walker.CurrentPathArray))
                {
                    // find bindings of current control
                    var bindableControl = (DotvvmBindableControl)control;
                    var binding = bindableControl.GetAllBindings().Where(p => p.Value is CommandBindingExpression)
                        .FirstOrDefault(b => b.Value.BindingId == commandId);
                    if (binding.Key != null)
                    {
                        // we have found the binding, now get the validation path
                        var currentValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(bindableControl);
                        if (currentValidationTargetPath == validationTargetPath)
                        {
                            // the validation path is equal, we have found the binding
                            resultBinding = (CommandBindingExpression)binding.Value;
                            resultControl = bindableControl;
                            resultProperty = binding.Key;
                        }
                    }
                }
            });

            return new FindBindingResult
            {
                Binding = resultBinding,
                Control = resultControl,
                Property = resultProperty
            };
        }

        /// <summary>
        /// Validates the control command.
        /// </summary>
        public FindBindingResult ValidateControlCommand(string[] path, string commandId, DotvvmControl viewRootControl, DotvvmControl targetControl, string validationTargetPath)
        {
            // find the binding
            var result = FindControlCommandBinding(path, commandId, viewRootControl, (DotvvmBindableControl)targetControl, validationTargetPath);
            if (result.Binding == null)
            {
                throw EventValidationException();
            }
            return result;
        }

        /// <summary>
        /// Finds the binding of the specified type on the specified viewmodel path.
        /// </summary>
        private FindBindingResult FindControlCommandBinding(string[] path, string commandId, DotvvmControl viewRootControl, DotvvmBindableControl targetControl, string validationTargetPath)
        {
            // walk the control tree and find the path
            ControlCommandBindingExpression resultBinding = null;
            DotvvmProperty resultProperty = null;
            DotvvmBindableControl resultControl = null;

            var walker = new ControlTreeWalker(viewRootControl);
            walker.ProcessControlTree((control) =>
            {
                // compare path
                if (control is DotvvmBindableControl && ViewModelPathComparer.AreEqual(path, walker.CurrentPathArray))
                {
                    // find bindings of current control
                    var bindableControl = (DotvvmBindableControl)control;
                    var binding = bindableControl.GetAllBindings().Where(p => p.Value is ControlCommandBindingExpression)
                        .FirstOrDefault(b => b.Value.BindingId == commandId);
                    if (binding.Key != null)
                    {
                        // verify that the target control is the control command target
                        if (bindableControl.GetClosestControlBindingTarget() == targetControl)
                        {
                            // we have found the binding, now get the validation path
                            var currentValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(bindableControl);
                            if (currentValidationTargetPath == validationTargetPath)
                            {
                                // the validation path is equal, we have found the binding
                                resultBinding = (ControlCommandBindingExpression)binding.Value;
                                resultProperty = binding.Key;
                                resultControl = bindableControl;
                            }
                        }
                    }
                }
            });

            return new FindBindingResult
            {
                Property = resultProperty,
                Binding = resultBinding,
                Control = resultControl
            };
        }

        /// <summary>
        /// Throws the event validation exception.
        /// </summary>
        private Exception EventValidationException()
            => new Exception("Illegal command invocation!");
    }

    public class FindBindingResult
    {
        public CommandBindingExpression Binding { get; set; }
        public DotvvmBindableControl Control { get; set; }
        public DotvvmProperty Property { get; set; }
    }
}