using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Runtime.Commands;

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
                throw EventValidationException(result?.ErrorMessage, result?.CandidateBindings);
            }

            // validate the command against the control
            if (result.Control is IEventValidationHandler)
            {
                if (!((IEventValidationHandler)result.Control).ValidateCommand(result.Property))
                {
                    throw EventValidationException(result?.ErrorMessage, result?.CandidateBindings);
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

            bool bindingInPath = false;
            var candidateBindings = new Dictionary<string, CandidateBindings>();

            var walker = new ControlTreeWalker(viewRootControl);
            walker.ProcessControlTree((control) =>
            {
                if (resultBinding == null)
                {
                    // find bindings of current control
                    var bindings = control.GetAllBindings()
                        .Where(b => b.Value is CommandBindingExpression);
                    string exceptionPropertyKey = null;
                    foreach (var binding in bindings)
                    {
                        StringBuilder infoMessage = new StringBuilder();

                        // checking path
                        if (!ViewModelPathComparer.AreEqual(path, walker.CurrentPathArray))
                        {
                            exceptionPropertyKey = "DataContext path";
                            infoMessage.Append(
                                $"Expected DataContext path: '{string.Join("/", path)}' Command binding DataContext path: '{string.Join("/", walker.CurrentPathArray)}'");
                        }
                        else
                        {
                            //Found a binding in DataContext
                            bindingInPath = true;
                        }

                        //checking binding id
                        if (((CommandBindingExpression)binding.Value).BindingId != commandId)
                        {
                            exceptionPropertyKey = exceptionPropertyKey == null
                                ? "binding id"
                                : string.Join(", ", exceptionPropertyKey, "binding id");
                        }

                        //checking validation path
                        var currentValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(control);
                        if (currentValidationTargetPath != validationTargetPath)
                        {
                            exceptionPropertyKey = exceptionPropertyKey == null
                                ? "binding id"
                                : string.Join(", ", exceptionPropertyKey, "validation path");
                            infoMessage.Append($"Expected validation path: '{string.Join("/", validationTargetPath)}' Command binding validation path: '{string.Join("/", currentValidationTargetPath)}'");
                        }
                        if(exceptionPropertyKey == null)
                        {
                            //correct binding found
                            resultBinding = (CommandBindingExpression)binding.Value;
                            resultControl = control;
                            resultProperty = binding.Key;
                        }
                        else
                        {
                            exceptionPropertyKey = "Command bindings with wrong " + exceptionPropertyKey;
                            if (!candidateBindings.ContainsKey(exceptionPropertyKey))
                            {
                                candidateBindings.Add(exceptionPropertyKey, new CandidateBindings());
                            }
                            candidateBindings[exceptionPropertyKey].AddBinding(new KeyValuePair<string, IBinding>(infoMessage.ToString(), binding.Value));
                        }
                    }
                }
            });

            return new FindBindingResult
            {
                ErrorMessage = bindingInPath ? null : "Nothing was found in found inside specified DataContext, check if ViewModel is populated",
                CandidateBindings = candidateBindings,
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
                throw EventValidationException(result.ErrorMessage, result.CandidateBindings);
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
            DotvvmBindableObject resultControl = null;
            DotvvmProperty resultProperty = null;

            bool bindingInPath = false;
            bool isControl;
            string errorMessage = null;
            var candidateBindings = new Dictionary<string, CandidateBindings>();

            var walker = new ControlTreeWalker(viewRootControl);
            walker.ProcessControlTree((control) =>
            {
                if (resultBinding == null)
                {
                    // find bindings of current control
                    var bindings = control.GetAllBindings()
                        .Where(b => b.Value is CommandBindingExpression);
                    string exceptionPropertyKey = null;
                    foreach (var binding in bindings)
                    {
                        StringBuilder infoMessage = new StringBuilder();

                        // checking path
                        if (!ViewModelPathComparer.AreEqual(path, walker.CurrentPathArray))
                        {
                            exceptionPropertyKey = "DataContext path";
                            infoMessage.Append(
                                $"Expected DataContext path: '{string.Join("/", path)}' Command binding DataContext path: '{string.Join("/", walker.CurrentPathArray)}'");
                        }
                        else
                        {
                            //Found a binding in DataContext
                            bindingInPath = true;
                        }

                        //checking binding id
                        if (((CommandBindingExpression)binding.Value).BindingId != commandId)
                        {
                            exceptionPropertyKey = exceptionPropertyKey == null
                                ? "binding id"
                                : string.Join(", ", exceptionPropertyKey, "binding id");
                        }

                        //checking validation path
                        var currentValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(control);
                        if (currentValidationTargetPath != validationTargetPath)
                        {
                            exceptionPropertyKey = exceptionPropertyKey == null
                                ? "binding id"
                                : string.Join(", ", exceptionPropertyKey, "validation path");
                            infoMessage.Append(
                                $"Expected validation path: '{string.Join("/", validationTargetPath)}' Command binding validation path: '{string.Join("/", currentValidationTargetPath)}'");
                        }

                        //checking if binding is control binding
                        isControl = control.GetClosestControlBindingTarget() == targetControl;

                        if (exceptionPropertyKey == null && isControl)
                        {
                            //correct binding found
                            resultBinding = (CommandBindingExpression)binding.Value;
                            resultControl = control;
                            resultProperty = binding.Key;
                        }
                        else if (exceptionPropertyKey != null)
                        {
                            exceptionPropertyKey = (isControl ? "Control command bindings with wrong " : "Command bindings with wrong ") + exceptionPropertyKey;
                            if (!candidateBindings.ContainsKey(exceptionPropertyKey))
                            {
                                candidateBindings.Add(exceptionPropertyKey, new CandidateBindings());
                            }
                            candidateBindings[exceptionPropertyKey]
                                .AddBinding(new KeyValuePair<string, IBinding>(infoMessage.ToString(), binding.Value));
                        }
                        else
                        {
                            errorMessage = "Invalid command invocation (the binding is not control command binding)";
                        }
                    }
                }
            });

            return new FindBindingResult
            {
                ErrorMessage = bindingInPath ? errorMessage : "Nothing was found in found inside specified DataContext, check if ViewModel is populated",
                CandidateBindings = candidateBindings,
                Binding = resultBinding,
                Control = resultControl,
                Property = resultProperty
            };
        }



        /// <summary>
        /// Throws the event validation exception.
        /// </summary>
        private Exception EventValidationException(string errorMessage = null, Dictionary<string, CandidateBindings> data = null)
        {
            var e = new Exception(errorMessage == null ? "Invalid command invocation!" : errorMessage);
            if (data != null)
            {
                foreach (var bindings in data)
                {
                    e.Data.Add(bindings.Key, bindings.Value.ToString());
                }
            }
            return e;
        }
    }

    public class FindBindingResult
    {
        public string ErrorMessage { get; set; }
        public Dictionary<string, CandidateBindings> CandidateBindings { get; set; }

        public CommandBindingExpression Binding { get; set; }
        public DotvvmBindableObject Control { get; set; }
        public DotvvmProperty Property { get; set; }
    }
}