using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.ViewModel.Validation
{
    public static class ValidationErrorFactory
    {
        /// <summary>
        /// Adds a new validation error with the given message and attaches it to the provided viewmodel. 
        /// The target viewmodel must be reachable from the root viewmodel, otherwise the error won't be attached.
        /// </summary>
        /// <param name="vm">Viewmodel</param>
        /// <param name="message">Validation error message</param>
        public static ViewModelValidationError AddModelError<T>(this T vm, string message)
            where T : class, IDotvvmViewModel
            => vm.Context.AddModelError(vm, a => a, message);

        /// <summary>
        /// Adds a new validation error with the given message and attaches it to the property determined by the provided expression. 
        /// The target property must be reachable from the root viewmodel, otherwise the error won't be attached.
        /// </summary>
        /// <param name="vm">Viewmodel or one of its descendant (reachable objects)</param>
        /// <param name="expr">Expression that determines the target property from the provided object</param>
        /// <param name="message">Validation error message</param>
        public static ViewModelValidationError AddModelError<T, TProp>(this T vm, Expression<Func<T, TProp>> expr, string message)
            where T : IDotvvmViewModel
        {
            var error = CreateModelError(vm.Context.Configuration, vm, expr, message);
            vm.Context.ModelState.ErrorsInternal.Add(error);
            return error;
        }

        public static ViewModelValidationError CreateModelError<T, TProp>(this T vm, Expression<Func<T, TProp>> expr, string error)
            where T : IDotvvmViewModel =>
            CreateModelError(vm.Context.Configuration, vm, expr, error);

        public static ValidationResult CreateValidationResult<T>(this T vm, string error, params Expression<Func<T, object>>[] expressions)
            where T : IDotvvmViewModel =>
            CreateValidationResult(vm.Context.Configuration, error, expressions);


        private static JavascriptTranslator defaultJavaScriptTranslator = new JavascriptTranslator(
            Options.Create(new JavascriptTranslatorConfiguration()),
            new ViewModelSerializationMapper(new ViewModelValidationRuleTranslator(), new AttributeViewModelValidationMetadataProvider(), new DefaultPropertySerialization(), DotvvmConfiguration.CreateDefault()));

        public static ValidationResult CreateValidationResult<T>(ValidationContext validationContext, string error, params Expression<Func<T, object>>[] expressions)
        {
            if (validationContext.Items.TryGetValue(typeof(DotvvmConfiguration), out var obj) && obj is DotvvmConfiguration dotvvmConfiguration)
            {
                return CreateValidationResult(dotvvmConfiguration, error, (LambdaExpression[])expressions);
            }

            // Fallback to default version of JavaScriptTranslator
            return new ValidationResult ( error, expressions.Select(expr => GetPathFromExpression(defaultJavaScriptTranslator, expr)) );
        }

        public static ViewModelValidationError CreateModelError<T, TProp>(DotvvmConfiguration config, object? obj, Expression<Func<T, TProp>> expr, string error) =>
            CreateModelError(config, obj, (LambdaExpression)expr, error);

        public static ValidationResult CreateValidationResult<T>(DotvvmConfiguration config, string error, params Expression<Func<T, object>>[] expressions) =>
            CreateValidationResult(config, error, (LambdaExpression[])expressions);

        public static ViewModelValidationError CreateModelError(DotvvmConfiguration config, object? obj, LambdaExpression expr, string error) =>
            new ViewModelValidationError(error, GetPathFromExpression(config, expr), obj);

        public static ValidationResult CreateValidationResult(DotvvmConfiguration config, string error, LambdaExpression[] expr) =>
            new ValidationResult(
                error,
                expr.Select(e => GetPathFromExpression(config, e)).ToArray()
            );

        private static ConcurrentDictionary<(JavascriptTranslator translator, LambdaExpression expression), string> exprCache =
            new ConcurrentDictionary<(JavascriptTranslator, LambdaExpression), string>(new TupleComparer<JavascriptTranslator, LambdaExpression>(null, ExpressionComparer.Instance));

        public static string GetPathFromExpression(DotvvmConfiguration config, LambdaExpression expr) =>
            GetPathFromExpression(config.ServiceProvider.GetRequiredService<JavascriptTranslator>(), expr, config.Debug);

        public static string GetPathFromExpression(JavascriptTranslator translator,
            LambdaExpression expr,
            bool isDebug = false)
        {
            expr = (LambdaExpression)new LocalVariableExpansionVisitor().Visit(expr);
            return exprCache.GetOrAdd((translator, expr), e => {
                var dataContext = DataContextStack.Create(e.expression.Parameters.Single().Type);
                var expression = ExpressionUtils.Replace(e.expression, BindingExpressionBuilder.GetParameters(dataContext).First(p => p.Name == "_this"));
                var jsast = translator.CompileToJavascript(expression, dataContext);

                var visitor = new PropertyPathExtractingVisitor();
                jsast.AcceptVisitor(visitor);
                return visitor.GetPropertyPath();
            });
        }

        private class LocalVariableExpansionVisitor : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression node)
            {
                var localValue = Expand(node);
                if (localValue != null)
                {
                    return Expression.Constant(localValue, node.Type);
                }
                return base.VisitMember(node);
            }

            private object? Expand(Expression? current)
            {
                if (current is ConstantExpression constant)
                {
                    return constant.Value;
                }
                if (current is not MemberExpression member)
                {
                    return null;
                }

                var inner = Expand(member.Expression);
                if (inner == null)
                {
                    return null;
                }

                if (member.Member is FieldInfo field)
                {
                    return field.GetValue(inner);
                }
                if (member.Member is PropertyInfo property)
                {
                    return property.GetValue(inner);
                }

                return null;
            }
        }
    }
}
