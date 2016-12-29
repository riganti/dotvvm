using System;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Runtime.Commands
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
        private FindBindingResult FindCommandBinding(string[] path, string commandId, DotvvmBindableObject viewRootControl, string validationTargetPath)
        {
            // walk the control tree and find the path
            CommandBindingExpression resultBinding = null;
            DotvvmBindableObject resultControl = null;
            DotvvmProperty resultProperty = null;

            var walker = new ControlTreeWalker(viewRootControl);
            walker.ProcessControlTree((control) =>
            {
                // compare path
                if (resultBinding == null && ViewModelPathComparer.AreEqual(path, walker.CurrentPathArray))
                {
                    // find bindings of current control
                    var binding = control.GetAllBindings().Where(p => p.Value is CommandBindingExpression)
                        .FirstOrDefault(b => b.Value.BindingId == commandId);
                    if (binding.Key != null)
                    {
                        // we have found the binding, now get the validation path
                        var currentValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(control);
                        if (currentValidationTargetPath == validationTargetPath)
                        {
                            // the validation path is equal, we have found the binding
                            resultBinding = (CommandBindingExpression)binding.Value;
                            resultControl = control;
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
            var result = FindControlCommandBinding(path, commandId, viewRootControl, targetControl, validationTargetPath);
            if (result.Binding == null)
            {
                throw EventValidationException();
            }
            return result;
        }

        /// <summary>
        /// Finds the binding of the specified type on the specified viewmodel path.
        /// </summary>
        private FindBindingResult FindControlCommandBinding(string[] path, string commandId, DotvvmControl viewRootControl, DotvvmBindableObject targetControl, string validationTargetPath)
        {
            // walk the control tree and find the path
            CommandBindingExpression resultBinding = null;
            DotvvmProperty resultProperty = null;
            DotvvmBindableObject resultControl = null;

            var walker = new ControlTreeWalker(viewRootControl);
            walker.ProcessControlTree((control) =>
            {
                // compare path
                if (ViewModelPathComparer.AreEqual(path, walker.CurrentPathArray))
                {
                    // find bindings of current control
                    var binding = control.GetAllBindings().Where(p => p.Value is CommandBindingExpression)
                        .FirstOrDefault(b => b.Value.BindingId == commandId);
                    if (binding.Key != null)
                    {
                        // verify that the target control is the control command target
                        if (control.GetClosestControlBindingTarget() == targetControl)
                        {
                            // we have found the binding, now get the validation path
                            var currentValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(control);
                            if (currentValidationTargetPath == validationTargetPath)
                            {
                                // the validation path is equal, we have found the binding
                                resultBinding = (CommandBindingExpression)binding.Value;
                                resultProperty = binding.Key;
                                resultControl = control;
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
        public DotvvmBindableObject Control { get; set; }
        public DotvvmProperty Property { get; set; }
    }
}