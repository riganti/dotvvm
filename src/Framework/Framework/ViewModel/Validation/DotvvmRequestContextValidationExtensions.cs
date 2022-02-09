using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ViewModel.Validation
{
    public static class DotvvmRequestContextValidationExtensions
    {
        public static ViewModelValidationError AddModelError(this IDotvvmRequestContext context, string message)
        {
            var error = new ViewModelValidationError(message)
            {
                IsResolved = true,
                PropertyPath = "/"
            };
            context.ModelState.ErrorsInternal.Add(error);
            return error;
        }

        public static ViewModelValidationError AddRawModelError(this IDotvvmRequestContext context, string absolutePath, string message)
        {
            EnsurePathIsRooted(absolutePath);
            var error = new ViewModelValidationError(message)
            {
                IsResolved = true,
                PropertyPath = absolutePath ?? "/"
            };
            context.ModelState.ErrorsInternal.Add(error);
            return error;
        }

        public static ViewModelValidationError AddModelError<T, TProp>(this IDotvvmRequestContext context, T vm, Expression<Func<T, TProp>> expression, string message)
        {
            var lambdaExpression = (LambdaExpression)expression;
            var propertyPath = ValidationErrorFactory.GetPathFromExpression(context.Configuration, lambdaExpression);

            var error = new ViewModelValidationError(message)
            {
                IsResolved = false,
                TargetObject = vm,
                PropertyPath = propertyPath
            };
            context.ModelState.ErrorsInternal.Add(error);
            return error;
        }

        private static void EnsurePathIsRooted(string propertyPath)
        {
            if (propertyPath != null && !propertyPath.StartsWith("/"))
                throw new ArgumentException("Hand-written paths need to be specified from the root of viewModel! Consider passing an expression (lambda) instead.");
        }
    }
}
