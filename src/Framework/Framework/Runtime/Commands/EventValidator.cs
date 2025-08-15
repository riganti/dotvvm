using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

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
        public FindBindingResult ValidateCommand(string[] path, string commandId, DotvvmControl viewRootControl, string? validationTargetPath)
        {
            // find the binding
            var result = FindCommandBinding(path, commandId, viewRootControl, validationTargetPath);
            if (result == null || result.Binding == null)
            {
                throw EventValidationException(result?.ErrorMessage, result?.CandidateBindings);
            }

            // validate the command against the control
            if (result.Control is IEventValidationHandler validationHandler)
            {
                if (!validationHandler.ValidateCommand(result.Property!))
                {
                    var config = (viewRootControl.GetValue(Internal.RequestContextProperty) as IDotvvmRequestContext)?.Configuration;
                    var message = $"Execution of '{result.Binding}' was disallowed by '{result.Control.DebugString(config, multiline: false)}'.";
                    throw EventValidationException(message, result?.CandidateBindings);
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

            var candidateBindings = new Dictionary<FindBindingResult.BindingMatchChecklist, CandidateBindings>();
            var infoMessage = new StringBuilder();
 
            var walker = new ControlTreeWalker(viewRootControl);
            walker.ProcessControlTree((control) =>
            {
                if (resultBinding == null)
                {
                    // find bindings of current control

                    foreach (var (propertyId, rawValue) in control.properties)
                    {
                        if (rawValue is not CommandBindingExpression binding)
                            continue;
                        var property = propertyId.PropertyInstance;


                        infoMessage.Clear();
                        var bindingMatch = new FindBindingResult.BindingMatchChecklist();

                        // checking path
                        if (!ViewModelPathComparer.AreEqual(path, walker.CurrentPathArray))
                        {
                            infoMessage.Append(
                                $"Expected DataContext path: '{string.Join("/", path)}' Command binding DataContext path: '{string.Join("/", walker.CurrentPathArray)}'");
                        }
                        else
                        {
                            bindingMatch.DataContextPathMatch = true;
                        }

                        //checking binding id
                        if (binding.BindingId == commandId)
                        {
                            bindingMatch.BindingIdMatch = true;
                        }

                        //checking validation path
                        var currentValidationTargetPath = KnockoutHelper.GetValidationTargetExpression(control)?.identificationExpression;
                        if (currentValidationTargetPath != validationTargetPath)
                        {
                            if (infoMessage.Length > 0)
                                infoMessage.Append("; ");
                            infoMessage.Append($"Expected validation path: '{validationTargetPath}' Command binding validation path: '{currentValidationTargetPath}'");
                        }
                        else
                        {
                            bindingMatch.ValidationPathMatch = true;
                        }

                        //If finding control command binding checks if the binding is control otherwise always true
                        bindingMatch.ControlMatch = !findControl || control.GetClosestControlBindingTarget() == targetControl;

                        if (!bindingMatch.ControlMatch)
                        {
                            if (infoMessage.Length > 0)
                            {
                                infoMessage.Append("; different markup control");
                            }
                            else
                            {
                                infoMessage.Append($"Expected control: '{(targetControl as DotvvmControl)?.GetDotvvmUniqueId()}' Command binding control: '{(control.GetClosestControlBindingTarget() as DotvvmControl)?.GetDotvvmUniqueId()}'");
                            }
                        }

                        if(bindingMatch.AllMatches)
                        {
                            //correct binding found
                            resultBinding = binding;
                            resultControl = control;
                            resultProperty = property;
                        }
                        else
                        {
                            // only add information about ID mismatch if no other mismatch was found to avoid information clutter
                            if (!bindingMatch.BindingIdMatch && infoMessage.Length == 0)
                            {
                                infoMessage.Append($"Expected internal binding id: '{commandId}' Command binding id: '{binding.BindingId}'");
                            }
                            if (!candidateBindings.ContainsKey(bindingMatch))
                            {
                                candidateBindings.Add(bindingMatch, new CandidateBindings());
                            }
                            candidateBindings[bindingMatch]
                                .AddBinding(new(infoMessage.ToString(), binding));
                        }
                    }
                }
            });

            string? errorMessage = null;
            if (candidateBindings.ContainsKey(new FindBindingResult.BindingMatchChecklist { ControlMatch = false, BindingIdMatch = true, DataContextPathMatch = true, ValidationPathMatch = true }))
            {
                // all properties match except the control
                errorMessage = "Invalid command invocation - The binding is not control command binding.";
            }
            else if (candidateBindings.All(b => !b.Key.DataContextPathMatch))
            {
                // nothing in the specified data context path
                errorMessage = $"Invalid command invocation - No commands were found inside DataContext '{string.Join("/", path)}'. Please check if ViewModel is populated.";
            }
            else
            {
                errorMessage = "Invalid command invocation - The specified command binding was not found.";
            }

            return new FindBindingResult
            {
                ErrorMessage = errorMessage,
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
            DotvvmBindableObject viewRootControl, string? validationTargetPath)
            => FindCommandBinding(path, commandId, viewRootControl, null, validationTargetPath, false);

        /// <summary>
        /// Validates the control command.
        /// </summary>
        public FindBindingResult ValidateControlCommand(string[] path, string commandId, DotvvmControl viewRootControl, DotvvmControl targetControl, string? validationTargetPath)
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
            DotvvmControl viewRootControl, DotvvmBindableObject targetControl, string? validationTargetPath)
            => FindCommandBinding(path, commandId, viewRootControl, targetControl, validationTargetPath, true);


        /// <summary>
        /// Throws the event validation exception.
        /// </summary>
        private InvalidCommandInvocationException EventValidationException(string? errorMessage = null, Dictionary<FindBindingResult.BindingMatchChecklist, CandidateBindings>? data = null)
        {
            var stringifiedData =
                data?.OrderByDescending(k => (k.Key.BindingIdMatch, k.Key.ControlMatch, -k.Key.MismatchCount))
                     .Select(k => new KeyValuePair<string, string[]>(k.Key.ToString(), k.Value.BindingsToString()))
                     .ToArray();
            return new InvalidCommandInvocationException(errorMessage ?? "Invalid command invocation!", stringifiedData);
        }
    }

    public class FindBindingResult
    {
        public string? ErrorMessage { get; set; }
        public Dictionary<BindingMatchChecklist, CandidateBindings> CandidateBindings { get; set; } = new();

        public CommandBindingExpression? Binding { get; set; }
        public DotvvmBindableObject? Control { get; set; }
        public DotvvmProperty? Property { get; set; }

        public struct BindingMatchChecklist: IEquatable<BindingMatchChecklist>
        {
            public bool ValidationPathMatch { get; set; }
            public bool BindingIdMatch { get; set; }
            public bool DataContextPathMatch { get; set; }
            public bool ControlMatch { get; set; }

            public readonly bool AllMatches => ValidationPathMatch && BindingIdMatch && DataContextPathMatch && ControlMatch;
            public readonly int MismatchCount => (ValidationPathMatch ? 0 : 1) + (BindingIdMatch ? 0 : 1) + (DataContextPathMatch ? 0 : 1) + (ControlMatch ? 0 : 1);

            public readonly bool Equals(BindingMatchChecklist other) => ValidationPathMatch == other.ValidationPathMatch && BindingIdMatch == other.BindingIdMatch && DataContextPathMatch == other.DataContextPathMatch && ControlMatch == other.ControlMatch;
            public readonly override bool Equals(object? obj) => obj is BindingMatchChecklist other && Equals(other);
            public readonly override int GetHashCode() => (ValidationPathMatch, BindingIdMatch, DataContextPathMatch, ControlMatch).GetHashCode();

            public readonly override string ToString()
            {
                if (AllMatches)
                {
                    return "Matching binding";
                }
                if (!ControlMatch && ValidationPathMatch && BindingIdMatch && DataContextPathMatch)
                {
                    return "The binding is not control command binding or is in wrong control";
                }
                if (!ValidationPathMatch && !BindingIdMatch && !DataContextPathMatch)
                {
                    return "No matching property";
                }
                var properties = new [] {
                    ("binding id", this.BindingIdMatch),
                    ("validation path", this.ValidationPathMatch),
                    ("DataContext path", this.DataContextPathMatch)
                }.ToLookup(p => p.Item2, p => p.Item1);

                var wrong = string.Join(", ", properties[false]);
                var correct = string.Join(", ", properties[true]);

                return "Command binding with wrong " + wrong + (correct.Any() ? " and correct " + correct : "");
            }
        }
    }
}
