using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class GenericMethodCompiler: IJsMethodTranslator
    {
        public Func<JsExpression[], JsExpression> MethodBuilder { get; set; }
		public Func<MethodInfo, Expression, Expression[], bool> CanTranslateDelegate { get; set; }

		public JsExpression TranslateCall(JsExpression context, JsExpression[] arguments, MethodInfo method)
        {
            return MethodBuilder(new[] { context }.Concat(arguments).ToArray());
        }

		public bool CanTranslateCall(MethodInfo method, Expression context, Expression[] arguments)
		{
			return CanTranslateDelegate == null ? true : CanTranslateDelegate(method, context, arguments);
		}

		public GenericMethodCompiler(Func<JsExpression[], JsExpression> builder, Func<MethodInfo, Expression, Expression[], bool> check = null)
        {
            MethodBuilder = builder;
			CanTranslateDelegate = check;
        }
    }
}
