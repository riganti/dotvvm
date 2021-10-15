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
            var error = new ViewModelValidationError(message);
            context.ModelState.ErrorsInternal.Add(error);
            error.IsResolved = true;
            return error;
        }

        public static ViewModelValidationError AddModelError(this IDotvvmRequestContext context, string propertyPath, string message)
        {
            EnsurePathIsRooted(propertyPath);
            var error = new ViewModelValidationError(message, propertyPath);
            context.ModelState.ErrorsInternal.Add(error);
            error.IsResolved = true;
            return error;
        }

        private static void EnsurePathIsRooted(string propertyPath)
        {
            if (propertyPath.Length == 0 || !propertyPath.StartsWith("/"))
                throw new ArgumentException("Hand-written paths need to be specified from the root of viewModel!");
        }

        // TODO: we might add this API in future:

        //public static ViewModelValidationError AddModelError<T, TProp>(this IDotvvmRequestContext context, T vm, Expression<Func<T, TProp>> expr, string message)
        //    where T : class
        //{
        //    var error = ValidationErrorFactory.CreateModelError(context.Configuration, vm, expr, message);
        //    context.ModelState.ErrorsInternal.Add(error);
        //    return error;
        //}
    }
}
