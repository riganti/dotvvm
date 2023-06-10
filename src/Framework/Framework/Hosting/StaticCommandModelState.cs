using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Framework.Hosting
{
    /// <summary>
    /// A class holding validation errors for static command arguments. Usage example:
    /// <code>
    /// var m = new StaticCommandModelState();
    /// m.AddArgumentError(() => arg.Name, "Name is invalid");
    /// m.FailOnInvalidModelState();
    /// </code>
    /// </summary>
    public class StaticCommandModelState
    {
        /// <summary>
        /// Gets a collection of validation errors for static command arguments
        /// </summary>
        public IReadOnlyList<StaticCommandValidationError> Errors => ErrorsInternal.AsReadOnly();
        internal List<StaticCommandValidationError> ErrorsInternal = new();

        /// <summary>
        /// Gets a value indicating whether the StaticCommandModelState is valid (i.e. does not contain any errors)
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// Adds a new validation error with the given message on the argument determined by its name
        /// </summary>
        /// <param name="argumentName">Name of argument determining where to attach error</param>
        /// <param name="message">Validation error message</param>
        public StaticCommandValidationError AddArgumentError(string argumentName, string message)
        {
            var error = new StaticCommandValidationError(message, argumentName) {
                ArgumentName = argumentName,
                ErrorMessage = message,
                IsResolved = false
            };

            ErrorsInternal.Add(error);
            return error;
        }

        /// <summary>
        /// Adds a new validation error with the given message on a property in the specified argument
        /// </summary>
        /// <param name="argumentName">Name of argument determining where to attach error</param>
        /// <param name="propertyPath">Property path determining the property where to attach error. Format example: `MyProperty1/NestedProperty` </param>
        /// <param name="message">Validation error message</param>
        public StaticCommandValidationError AddRawArgumentError(string argumentName, string propertyPath, string message)
        {
            var error = new StaticCommandValidationError(message, argumentName) {
                ArgumentName = argumentName,
                PropertyPath = propertyPath,
                ErrorMessage = message,
                IsResolved = false
            };

            ErrorsInternal.Add(error);
            return error;
        }

        /// <summary>
        /// Adds a new validation error with the given message on a any property of the viewmodel. The path is *not* relative to the staticCommand, it is an absolute path in the page viewmodel
        /// </summary>
        /// <param name="propertyPath">Property path determining the property where to attach error. Format example: `/MyProperty1/MyCollection/3/NestedProperty2` </param>
        /// <param name="message">Validation error message</param>
        public StaticCommandValidationError AddRawError(string propertyPath, string message)
        {
            var error = new StaticCommandValidationError(message) {
                PropertyPath = propertyPath,
                ErrorMessage = message,
                IsResolved = true
            };

            ErrorsInternal.Add(error);
            return error;
        }

        /// <summary>
        /// Adds a new validation error with the given message on the argument (or its property) determined by an expression
        /// </summary>
        /// <param name="expression">Expression that determines the target property from an argument</param>
        /// <param name="message">Validation error message</param>
        public StaticCommandValidationError AddArgumentError<TProp>(Expression<Func<TProp>> expression, string message)
        {
            [DoesNotReturn]
            void ThrowCouldNotExtractProperyInfo()
                => throw new ArgumentException($"Can not get property info from {expression.Body}");

            var targetObjectType = expression.Body switch {
                IndexExpression indexExpression => indexExpression.Type,
                MemberExpression memberExpression => memberExpression.Expression?.Type,
                _ => null,
            };

            if (targetObjectType == null)
                ThrowCouldNotExtractProperyInfo();

            var member = ((MemberExpression)expression.Body).Member;
            var paramThis = Expression.Parameter(targetObjectType, "_this");
            var parameter = Expression.Parameter(targetObjectType, member.Name);
            var lambda = Expression.Lambda(parameter, paramThis, parameter);

            var error = new StaticCommandValidationError(message) {
                ErrorMessage = message,
                PropertyPathExtractor = (dotvvmConfig) => ValidationErrorFactory.GetPathFromExpression(dotvvmConfig, lambda, expandLocals: false)
            };

            ErrorsInternal.Add(error);
            return error;
        }

        /// <summary>
        /// Interrupts execution of the static command if this instance contains validation errors
        /// </summary>
        public void FailOnInvalidModelState()
        {
            if (!IsValid)
                throw new DotvvmInvalidStaticCommandModelStateException(this);
        }

        /// <summary>
        /// Removes all errors
        /// </summary>
        public void ClearErrors()
        {
            ErrorsInternal.Clear();
        }
    }
}
