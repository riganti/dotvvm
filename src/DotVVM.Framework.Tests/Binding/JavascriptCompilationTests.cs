using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Compilation;
using DotVVM.Framework.Runtime.Compilation.Binding;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;
using DotVVM.Framework.Runtime.ControlTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class JavascriptCompilationTests
    {
        public object CompileBinding(string expression, params Type[] contexts)
        {
            var context = new DataContextStack(contexts.FirstOrDefault() ?? typeof(object));
            context.RootControlType = typeof(DotvvmControl);
            for (int i = 1; i < contexts.Length; i++)
            {
                context = new DataContextStack(contexts[i], context);
            }
            var parser = new CompileTimeBindingParser();
            var expressionTree = parser.Parse(expression, context, BindingParserOptions.Create<ValueBindingExpression>());
            return JavascriptTranslator.CompileToJavascript(expressionTree, context);
        }

        [TestMethod]
        public void JavascriptCompilation_EnumComparison()
        {
            var js = CompileBinding($"_this == 'Local'", typeof(DateTimeKind));
            Assert.AreEqual("$data==2", js);
        }
    }
}
