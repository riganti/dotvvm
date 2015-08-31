using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Compilation;
using DotVVM.Framework.Runtime.Compilation.Binding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class BidningCompilationTests
    {
        public object ExecuteBinding(string expression, object[] contexts, DotvvmControl control)
        {
            var context = new DataContextStack(contexts.FirstOrDefault()?.GetType() ?? typeof(object));
            context.RootControlType = control?.GetType() ?? typeof(DotvvmControl);
            for (int i = 1; i < contexts.Length; i++)
            {
                context = new DataContextStack(contexts[i].GetType(), context);
            }
            var parser = new CompileTimeBindingParser();
            var expressionTree = parser.Parse(expression, context);
            return BindingCompiler.CompileToDelegate(expressionTree, context).Compile()(contexts, control);
        }

        public object ExecuteBinding(string expression, params object[] contexts)
        {
            return ExecuteBinding(expression, contexts, null);
        }

        [TestMethod]
        public void BindingCompiler_Valid_JustProperty()
        {
            var viewModel = new TestViewModel() { StringProp = "abc" };
            Assert.AreEqual(ExecuteBinding("StringProp", viewModel), "abc");
        }

        [TestMethod]
        public void BindingCompiler_Valid_StringConcat()
        {
            var viewModel = new TestViewModel() { StringProp = "abc" };
            Assert.AreEqual(ExecuteBinding("StringProp + \"d\"", viewModel), "abcd");
        }

        [TestMethod]
        public void BindingCompiler_Valid_StringLiteralInSingleQuotes()
        {
            var viewModel = new TestViewModel() { StringProp = "abc" };
            Assert.AreEqual(ExecuteBinding("StringProp + 'def'", viewModel), "abcdef");
        }

        [TestMethod]
        public void BindingCompiler_Valid_PropertyProperty()
        {
            var viewModel = new TestViewModel() { StringProp = "abc", TestViewModel2 = new TestViewModel2 { MyProperty = 42 } };
            Assert.AreEqual(ExecuteBinding("TestViewModel2.MyProperty", viewModel), 42);
        }

        [TestMethod]
        public void BindingCompiler_Valid_MethodCall()
        {
            var viewModel = new TestViewModel { StringProp = "abc" };
            Assert.AreEqual(ExecuteBinding("SetStringProp('hulabula', 13)", viewModel), "abc");
            Assert.AreEqual(viewModel.StringProp, "hulabula13");
        }

        [TestMethod]
        public void BindingCompiler_Valid_EnumStringComparison()
        {
            var viewModel = new TestViewModel { EnumProperty = TestEnum.A };
            Assert.AreEqual(ExecuteBinding("EnumProperty == 'A'", viewModel), true);
            Assert.AreEqual(ExecuteBinding("EnumProperty == 'B'", viewModel), false);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void BindingCompiler_Invalid_EnumStringComparison()
        {
            var viewModel = new TestViewModel { EnumProperty = TestEnum.A };
            ExecuteBinding("Enum == 'ghfjdskdjhbvdksdj'", viewModel);
        }

        class TestViewModel
        {
            public string StringProp { get; set; }

            public TestViewModel2 TestViewModel2 { get; set; }
            public TestEnum EnumProperty { get; set; }

            public string SetStringProp(string a, int b)
            {
                var p = StringProp;
                StringProp = a + b;
                return p;
            }
        }
        enum TestEnum
        {
            A,
            B,
            C,
            D
        }

        class TestViewModel2
        {
            public int MyProperty { get; set; }

        }
    }
}
