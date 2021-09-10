using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class GenericMethodCompiler : IJavascriptMethodTranslator
    {
        public Func<LazyTranslatedExpression?, LazyTranslatedExpression[], MethodInfo, JsExpression?> TryTranslateDelegate;

        public JsExpression? TryTranslateCall(LazyTranslatedExpression? context, LazyTranslatedExpression[] arguments, MethodInfo method) =>
            TryTranslateDelegate(context, arguments, method);

        public GenericMethodCompiler(Func<JsExpression[], JsExpression> builder, Func<MethodInfo, Expression?, Expression[], bool>? check = null)
        {
            TryTranslateDelegate =
                (t, arg, m) => check?.Invoke(m, t?.OriginalExpression, arg.Select(a => a.OriginalExpression).ToArray()) == false
                ? null
                : builder(new [] { t?.JsExpression()! }.Concat(arg.Select(a => a.JsExpression())).ToArray());
        }

        public GenericMethodCompiler(Func<JsExpression[], Expression[], JsExpression> builder, Func<MethodInfo, Expression?, Expression[], bool>? check = null)
        {
            TryTranslateDelegate =
                (t, arg, m) => check?.Invoke(m, t?.OriginalExpression, arg.Select(a => a.OriginalExpression).ToArray()) == false
                ? null
                : builder(new[] { t?.JsExpression()! }.Concat(arg.Select(a => a.JsExpression())).ToArray(), arg.Select(a => a.OriginalExpression).ToArray());
        }

        public GenericMethodCompiler(Func<JsExpression[], MethodInfo, JsExpression> builder, Func<MethodInfo, Expression?, Expression[], bool>? check = null)
        {
            TryTranslateDelegate =
                (t, arg, m) => check?.Invoke(m, t?.OriginalExpression, arg.Select(a => a.OriginalExpression).ToArray()) == false
                ? null
                : builder(new [] { t?.JsExpression()! }.Concat(arg.Select(a => a.JsExpression())).ToArray(), m);
        }
    }
}
