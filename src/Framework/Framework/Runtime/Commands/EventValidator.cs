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
                if (!((IEventValidationHandler)result.Control).ValidateCommand(result.Property!))
                {
                    throw EventValidationException(result?.ErrorMessage, result?.CandidateBindings);
                }
            }
            return result;
        }

        /// <summary>
        /// Finds the binding of the specified type on the specified viewmodel path.
        /// </summary>
        /// <param name="path">DataContext path of the binding</param>
        /// <param name="commandId">Id of the binding</param>
        /// <param name="viewRootControl">ViewRootControl of the binding</param>
        /// <param name="targetControl">Target control of the binding, null if not finding control command binding</param>
        /// <param name="validationTargetPath">Validation path of the binding</param>
        /// <param name="findControl">Determinate whether finding control command binding or not</param>
        /// <returns></returns>
        private FindBindingResult FindCommandBinding(string[] path, string commandId, 
            DotvvmBindableObject viewRootControl, DotvvmBindableObject? targetControl, 
            string? validationTargetPath, bool findControl)
        {
            // walk the control tree and find the path
            CommandBindingExpression? resultBinding = null;
            DotvvmBindableObject? resultControl = null;
            DotvvmProperty? resultProperty = null;

            bool checkControl;
            bool bindingInPath = false;
            var candidateBindings = new Dictionary<string, CandidateBindings>();
            string? errorMessage = null;

            var walker = new ControlTreeWalker(viewRootControl);
            walker.ProcessControlTree((control) =>
            {
                if (resultBinding == null)
                {
                    // find bindings of current control
                    var bindings = control.GetAllBindings()
                        .Where(b => b.Value is CommandBindingExpression);

                    foreach (var binding in bindings)
                    {
                        var wrongExceptionPropertyKeys = new List<string>();
                        var correctExceptionPropertyKeys = new List<string>();
                        var infoMessage = new StringBuilder();

                        // checking path
                        if (!ViewModelPathComparer.AreEqual(path, walker.CurrentPathArray))
                        {
                            wrongExceptionPropertyKeys.Add("DataContext path");
                            infoMessage.Append(
                                $"Expected DataContext path: '{string.Join("/", path)}' Command binding DataContext path: '{string.Join("/", walker.CurrentPathArray)}'");
                        }
                        else
                        {
                            //Found a binding in DataContext
                            bindingInPath = true;
                            correctExceptionPropertyKeys.Add("DataContext path");
                        }

                        //checking binding id
                        if (((CommandBindingExpression) binding.Value).BindingId != commandId)
                        {
                            wrongExceptionPropertyKeys.Add("binding id");
                        }
                        else
                        {
                            correctExceptionPropertyKeys.Add("binding id");
                        }

                        //checking validation path
                        var currentValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(control)?.identificationExpression;
                        if (currentValidationTargetPath != validationTargetPath)
                        {
                            wrongExceptionPropertyKeys.Add("validation path");
                            infoMessage.Append($"Expected validation path: '{string.Join("/", validationTargetPath)}' Command binding validation path: '{string.Join("/", currentValidationTargetPath)}'");
                        }
                        else
                        {
                            correctExceptionPropertyKeys.Add("validation path");
                        }

                        //If finding control command binding checks if the binding is control otherwise always true
                        checkControl = !findControl || control.GetClosestControlBindingTarget() == targetControl;

                        if(!wrongExceptionPropertyKeys.Any() && checkControl)
                        {
                            //correct binding found
                            resultBinding = (CommandBindingExpression)binding.Value;
                            resultControl = control;
                            resultProperty = binding.Key;
                        }
                        else if (wrongExceptionPropertyKeys.Any())
                        {
                            var exceptionPropertyKey =
                                (findControl && checkControl
                                    ? "Control command bindings with wrong "
                                    : "Command bindings with wrong ") + string.Join(", ", wrongExceptionPropertyKeys)
                                + (correctExceptionPropertyKeys.Any()
                                    ? " and correct " + string.Join(", ", correctExceptionPropertyKeys)
                                    : "")
                                + ":";
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
                ErrorMessage = bindingInPath ? errorMessage : "Nothing was found inside specified DataContext. Please check if ViewModel is populated.",
                CandidateBindings = candidateBindings,
                Binding = resultBinding,
                Control = resultControl,
                Property = resultProperty
            };
        }

        /// <summary>
        /// Finds the binding of the specified type on the specified viewmodel path.
        /// </summary>
        private FindBindingResult FindCommandBinding(string[] path, string commandId,
            DotvvmBindableObject viewRootControl, string validationTargetPath)
            => FindCommandBinding(path, commandId, viewRootControl, null, validationTargetPath, false);

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
        private FindBindingResult FindControlCommandBinding(string[] path, string commandId,
            DotvvmControl viewRootControl, DotvvmBindableObject targetControl, string validationTargetPath)
            => FindCommandBinding(path, commandId, viewRootControl, targetControl, validationTargetPath, true);


        /// <summary>
        /// Throws the event validation exception.
        /// </summary>
        private InvalidCommandInvocationException EventValidationException(string? errorMessage = null, Dictionary<string, CandidateBindings>? data = null)
            => new InvalidCommandInvocationException(errorMessage == null ? "Invalid command invocation!" : errorMessage, data);
    }

    public class FindBindingResult
    {
        public string? ErrorMessage { get; set; }
        public Dictionary<string, CandidateBindings> CandidateBindings { get; set; } = new();

        public CommandBindingExpression? Binding { get; set; }
        public DotvvmBindableObject? Control { get; set; }
        public DotvvmProperty? Property { get; set; }
    }
}
