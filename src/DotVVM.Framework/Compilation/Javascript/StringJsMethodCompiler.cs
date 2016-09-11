using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class StringJsMethodCompiler: IJsMethodTranslator
    {
        public string FormatString { get; set; }
		public Func<MethodInfo, Expression, Expression[], bool> CanTranslateDelegate { get; set; }

		public string TranslateCall(string context, string[] arguments, MethodInfo method)
        {
            return string.Format(FormatString, new[] { context }.Concat(arguments).ToArray());
        }

		public bool CanTranslateCall(MethodInfo method, Expression context, Expression[] arguments)
		{
			return CanTranslateDelegate == null ? true : CanTranslateDelegate(method, context, arguments);
		}

		public StringJsMethodCompiler(string formatString, Func<MethodInfo, Expression, Expression[], bool> check = null)
        {
            FormatString = formatString;
			CanTranslateDelegate = check;
        }
    }
}
