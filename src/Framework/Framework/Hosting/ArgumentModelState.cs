using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Framework.Hosting
{
    public class ArgumentModelState
    {
        /// <summary>
        /// Gets a collection of validation errors for arguments (used for static commands)
        /// </summary>
        public IReadOnlyList<StaticCommandArgumentValidationError> Errors => ErrorsInternal.AsReadOnly();
        internal List<StaticCommandArgumentValidationError> ErrorsInternal;

        public ArgumentModelState()
        {
            ErrorsInternal = new List<StaticCommandArgumentValidationError>();
        }

        /// <summary>
        /// Gets a value indicating whether the ArgumentModelState is valid (i.e. does not contain any errors)
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// Adds a new validation error with the given message on the argument determined by its name
        /// </summary>
        /// <param name="argumentName">Name of argument determining where to attach error</param>
        /// <param name="message">Validation error message</param>
        /// <returns></returns>
        public StaticCommandArgumentValidationError AddArgumentError(string argumentName, string message)
        {
            var error = new StaticCommandArgumentValidationError(message, argumentName) {
                IsResolved = false,
                ArgumentName = argumentName,
                ErrorMessage = message
            };

            ErrorsInternal.Add(error);
            return error;
        }

        public void FailOnInvalidModelState()
        {
            if (!IsValid)
                throw new DotvvmInvalidArgumentModelStateException(this);
        }
    }
}
