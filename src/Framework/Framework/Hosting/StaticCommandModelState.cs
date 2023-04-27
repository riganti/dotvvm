using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Framework.Hosting
{
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
        /// Adds a new validation error with the given message on a property of the viewmodel
        /// </summary>
        /// <param name="propertyPath">Property path determining the nested value where to attach error</param>
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

            var param = Expression.Parameter(targetObjectType, "_this");
            var lambda = Expression.Lambda(expression.Body, param);

            var error = new StaticCommandValidationError(message) {
                ErrorMessage = message,
                PropertyPathExtractor = (dotvvmConfig) => ValidationErrorFactory.GetPathFromExpression(dotvvmConfig, lambda)
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
