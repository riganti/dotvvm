using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Utils;
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
            var visitor = new PreprocessVisitor();
            var processedExpression = visitor.Visit(expression.Body);
            if (visitor.InroducedParameters.Count == 0)
                throw new Exception($"Expression {expression.Body} does not contain any references to method arguments");
            if (visitor.InroducedParameters.Count > 1)
                throw new Exception($"Expression {expression.Body} contains references to multiple method arguments ({string.Join(", ", visitor.InroducedParameters.Keys.Select(p => p.Name))})");

            var argumentParam = visitor.InroducedParameters.Values.Single();
            var paramThis = Expression.Parameter(argumentParam.Type, "_this");
            var lambda = Expression.Lambda(processedExpression, paramThis, argumentParam);

            var error = new StaticCommandValidationError(message) {
                ErrorMessage = message,
                PropertyPathExtractor = (dotvvmConfig) => ValidationErrorFactory.GetPathFromExpression(dotvvmConfig, lambda, expandLocals: false)
            };

            ErrorsInternal.Add(error);
            return error;
        }

        /// <summary> Replaces display class field accesses with ParameterExpression or ConstantExpressions, if used in indexer </summary>
        sealed class PreprocessVisitor: ExpressionVisitor
        {
            // In C#, when lambda `() => arg.MyProperty` is provided, we get Constant(displayClassInstance).arg.MyProperty where arg is a public field
            // In F#, we just get the constant, because this code drops the Value name: https://github.com/dotnet/fsharp/blob/3fd4b6594bf7b806a1ff9a29ec500956f2dc95b7/src/FSharp.Core/Linq.fs#L446
            //    we can't thus infer the argument name, so the AddRawArgumentError method must be used
            public Dictionary<FieldInfo, ParameterExpression> InroducedParameters = new();
            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member is FieldInfo field && node.Expression is ConstantExpression { Value: {} constant } && constant.GetType().IsClass)
                {
                    if (!InroducedParameters.TryGetValue(field, out var parameter))
                    {
                        parameter = Expression.Parameter(field.FieldType, field.Name);
                        InroducedParameters.Add(field, parameter);
                    }
                    return parameter;
                }
                return base.VisitMember(node);
            }


            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                // indexer, but for some reason as method call expression
                if (node.Object is {} && node.Arguments.Count == 1 && node.Method.IsSpecialName && node.Method.Name.StartsWith("get_") && node.Arguments[0].OptimizeConstants() is ConstantExpression constantIndex)
                {
                    return node.Update(Visit(node.Object), new[] { constantIndex });
                }
                return base.VisitMethodCall(node);
            }

            protected override Expression VisitIndex(IndexExpression node)
            {
                if (node.Object is {} && node.Arguments.Count == 1 && node.Arguments[0].OptimizeConstants() is ConstantExpression constantIndex)
                {
                    return node.Update(Visit(node.Object), new[] { constantIndex });
                }
                return base.VisitIndex(node);
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (node.NodeType == ExpressionType.ArrayIndex && node.Right.OptimizeConstants() is ConstantExpression constantIndex)
                {
                    return node.Update(Visit(node.Left), node.Conversion, constantIndex);
                }
                return base.VisitBinary(node);
            }

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
