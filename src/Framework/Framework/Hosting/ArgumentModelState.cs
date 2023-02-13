using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                ArgumentName = argumentName,
                ErrorMessage = message,
                IsResolved = false
            };

            ErrorsInternal.Add(error);
            return error;
        }

        public StaticCommandArgumentValidationError AddRawArgumentError(string argumentName, string propertyPath, string message)
        {
            var error = new StaticCommandArgumentValidationError(message, argumentName) {
                PropertyPath = propertyPath,
                ErrorMessage = message,
                IsResolved = true
            };

            ErrorsInternal.Add(error);
            return error;
        }

        ///// <summary>
        ///// Adds a new validation error with the given message on the argument determined by its name
        ///// </summary>
        ///// <param name="argumentName">Name of argument determining where to attach error</param>
        ///// <param name="expression">Expression that determines the target property from the provided object</param>
        ///// <param name="message">Validation error message</param>
        ///// <returns></returns>
        //public StaticCommandArgumentValidationError AddArgumentError<T, TProp>(string argumentName, Expression<Func<T, TProp>> expression, string message)
        //{
        //    var lambdaExpression = (LambdaExpression)expression;
        //    var propertyPath = ValidationErrorFactory.GetPathFromExpression(context.Configuration, lambdaExpression);

        //    var error = new StaticCommandArgumentValidationError(message, argumentName) {
        //        ArgumentName = argumentName,
        //        ErrorMessage = message,
        //        PropertyPath = propertyPath
        //    };

        //    ErrorsInternal.Add(error);
        //    return error;
        //}

        public void FailOnInvalidModelState()
        {
            if (!IsValid)
                throw new DotvvmInvalidArgumentModelStateException(this);
        }
    }
}
