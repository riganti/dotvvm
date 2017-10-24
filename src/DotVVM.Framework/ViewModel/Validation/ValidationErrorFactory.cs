using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.ViewModel.Validation
{
    public static class ValidationErrorFactory
    {
        public static void AddModelError<T, TProp>(this T vm, Expression<Func<T, TProp>> expr, string message)
            where T : class, IDotvvmViewModel
        {
            AddModelError(vm, expr, message, vm.Context);
        }

        public static void AddModelError<T, TProp>(T vm, Expression<Func<T, TProp>> expr, string message, IDotvvmRequestContext context)
            where T: class
        {
            if (context.ModelState.ValidationTarget != vm) throw new NotSupportedException($"ValidationTarget ({context.ModelState.ValidationTarget?.GetType().Name ?? "null"}) must be equal to specified view model ({vm.GetType().Name}).");
            var error = CreateModelError(context.Configuration, expr, message);
            context.ModelState.Errors.Add(error);
        }

        public static ViewModelValidationError CreateModelError<T, TProp>(T vm, Expression<Func<T, TProp>> expr, string error)
            where T : IDotvvmViewModel =>
            CreateModelError(vm.Context.Configuration, expr, error);

        public static ValidationResult CreateValidationResult<T>(T vm, string error, params Expression<Func<T, object>>[] expressions)
            where T : IDotvvmViewModel =>
            CreateValidationResult(vm.Context.Configuration, error, expressions);

        public static ViewModelValidationError CreateModelError<T, TProp>(DotvvmConfiguration config, Expression<Func<T, TProp>> expr, string error) =>
            CreateModelError(config, (LambdaExpression)expr, error);

        public static ValidationResult CreateValidationResult<T>(DotvvmConfiguration config, string error, params Expression<Func<T, object>>[] expressions) =>
            CreateValidationResult(config, error, (LambdaExpression[])expressions);
        public static ViewModelValidationError CreateModelError(DotvvmConfiguration config, LambdaExpression expr, string error) =>
            new ViewModelValidationError {
                ErrorMessage = error,
                PropertyPath = GetPathFromExpression(config, expr)
            };

        public static ValidationResult CreateValidationResult(DotvvmConfiguration config, string error, LambdaExpression[] expr) =>
            new ValidationResult(
                error,
                expr.Select(e => GetPathFromExpression(config, e)).ToArray()
            );

        private static ConcurrentDictionary<(DotvvmConfiguration config, LambdaExpression expression), string> exprCache =
            new ConcurrentDictionary<(DotvvmConfiguration, LambdaExpression), string>(new TupleComparer<DotvvmConfiguration, LambdaExpression>(null, ExpressionComparer.Instance));
        public static string GetPathFromExpression(DotvvmConfiguration config, LambdaExpression expr) =>
            exprCache.GetOrAdd((config, expr), e => {
                var dataContext = DataContextStack.Create(e.expression.Parameters.Single().Type);
                var expression = ExpressionUtils.Replace(e.expression, BindingExpressionBuilder.GetParameters(dataContext).First(p => p.Name == "_this"));
                var jsast = config.ServiceLocator.GetService<JavascriptTranslator>().CompileToJavascript(expression, dataContext);
                var pcode = BindingPropertyResolvers.FormatJavascript(jsast, niceMode: e.config.Debug, nullChecks: false);
                return JavascriptTranslator.FormatKnockoutScript(pcode, allowDataGlobal: true);
            });
    }
}
