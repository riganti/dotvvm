using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class JavascriptCompilationTests
    {
		public object CompileBinding(string expression, params Type[] contexts) => CompileBinding(expression, contexts, expectedType: typeof(object));
        public object CompileBinding(string expression, Type[] contexts, Type expectedType)
        {
            var context = new DataContextStack(contexts.FirstOrDefault() ?? typeof(object), rootControlType: typeof(DotvvmControl));
            for (int i = 1; i < contexts.Length; i++)
            {
                context = new DataContextStack(contexts[i], context);
            }
            var parser = new BindingExpressionBuilder();
			var expressionTree = TypeConversion.ImplicitConversion(parser.Parse(expression, context, BindingParserOptions.Create<ValueBindingExpression>()), expectedType, true, true);
            return JavascriptTranslator.CompileToJavascript(expressionTree, context);
        }

        [TestMethod]
        public void JavascriptCompilation_EnumComparison()
        {
            var js = CompileBinding($"_this == 'Local'", typeof(DateTimeKind));
            Assert.AreEqual("($data==\"Local\")", js);
        }

		[TestMethod]
		public void JavascriptCompilation_ConstantToString()
		{
			var js = CompileBinding("5", Type.EmptyTypes, typeof(string));
			Assert.AreEqual("\"5\"", js);
		}

		[TestMethod]
		public void JavascriptCompilation_ToString()
		{
			var js = CompileBinding("MyProperty", new[] { typeof(TestViewModel2) }, typeof(string));
			Assert.AreEqual("String($data.MyProperty())", js);
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void JavascriptCompilation_ToString_Invalid()
		{
			var js = CompileBinding("TestViewModel2", new[] { typeof(TestViewModel) }, typeof(string));
		}
	}
}
