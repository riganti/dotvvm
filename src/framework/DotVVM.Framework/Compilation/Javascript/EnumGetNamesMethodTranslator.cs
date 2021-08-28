using System;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class EnumGetNamesMethodTranslator : IJavascriptMethodTranslator
    {
        public JsExpression TryTranslateCall(LazyTranslatedExpression? context, LazyTranslatedExpression[] arguments, MethodInfo method)
        {
            var enumNames = (string[])method.Invoke(null, new object[0]);

            return new JsArrayExpression(enumNames.Select(n => new JsLiteral(n)))
                .WithAnnotation(new ViewModelInfoAnnotation(typeof(string[]), containsObservables: false));
        }
    }
}
