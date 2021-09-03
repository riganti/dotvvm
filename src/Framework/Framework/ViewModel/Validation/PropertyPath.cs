using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ViewModel.Serialization;
using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.ViewModel.Validation
{
    public static class PropertyPath
    {
        /// <summary>
        /// Builds a <see cref="ViewModelValidationError.PropertyPath"/> from a LINQ expression
        /// </summary>
        [Obsolete("You should rather use ValidationErrorFactory helpers")]
        public static string BuildPath<TValidationTarget>(Expression<Func<TValidationTarget, object>> propertyAccessor, DotvvmConfiguration configuration)
        {
            var context = DataContextStack.Create(typeof(TValidationTarget));
            var js = configuration.ServiceProvider.GetRequiredService<JavascriptTranslator>().CompileToJavascript(propertyAccessor, context);
            js.AcceptVisitor(new KnockoutObservableHandlingVisitor(true));
            var propertyPathExtractingVisitor = new PropertyPathExtractingVisitor();
            js.AcceptVisitor(propertyPathExtractingVisitor);
            return propertyPathExtractingVisitor.ExtractedPropertyPath;
        }
    }
}