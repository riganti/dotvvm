using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
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

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class BindingCompilationTests
    {
        public object ExecuteBinding(string expression, object[] contexts, DotvvmControl control, NamespaceImport[] imports = null, Type expectedType = null)
        {
            var context = new DataContextStack(contexts.FirstOrDefault()?.GetType() ?? typeof(object), rootControlType: control?.GetType() ?? typeof(DotvvmControl));
            for (int i = 1; i < contexts.Length; i++)
            {
                context = new DataContextStack(contexts[i].GetType(), context);
            }
            var parser = new BindingExpressionBuilder();
            var expressionTree = parser.Parse(expression, context, BindingParserOptions.Create<ValueBindingExpression>(importNs: new[] { new NamespaceImport("DotVVM.Framework.Tests.Binding") }.Concat(imports ?? Enumerable.Empty<NamespaceImport>()).ToArray()));
            return new BindingCompilationAttribute().CompileToDelegate(expressionTree, context, expectedType ?? typeof(object)).Compile()(contexts, control);
        }

        public object ExecuteBinding(string expression, params object[] contexts)
        {
            return ExecuteBinding(expression, contexts, null);
        }

        [TestMethod]
        public void BindingCompiler_FullNameResourceBinding()
        {
            Assert.AreEqual(Resource1.ResourceKey123, ExecuteBinding("DotVVM.Framework.Tests.Resource1.ResourceKey123"));
        }

        [TestMethod]
        public void BindingCompiler_NamespaceResourceBinding()
        {
            Assert.AreEqual(Resource1.ResourceKey123, ExecuteBinding("Resource1.ResourceKey123", new object[0], null, new NamespaceImport[]
            {
                new NamespaceImport("DotVVM.Framework.Tests")
            }));
        }

        [TestMethod]
        public void BindingCompiler_MoreNamespacesResourceBinding()
        {
            Assert.AreEqual(Resource1.ResourceKey123, ExecuteBinding("Resource1.ResourceKey123", new object[0], null, new NamespaceImport[]
            {
                new NamespaceImport("DotVVM.Framework.Tests0"),
                new NamespaceImport("DotVVM.Framework.Tests"),
                new NamespaceImport("DotVVM.Framework.Tests2")
            }));
        }

        [TestMethod]
        public void BindingCompiler_NamespaceAliasResourceBinding()
        {
            Assert.AreEqual(Resource1.ResourceKey123, ExecuteBinding("ghg.Resource1.ResourceKey123", new object[0], null, new NamespaceImport[]
            {
                new NamespaceImport("DotVVM.Framework.Tests","ghg")
            }));
        }

        [TestMethod]
        public void BindingCompiler_ResourceBindingException()
        {
            try
            {
                Assert.AreEqual(Resource1.ResourceKey123, ExecuteBinding("Resource1.NotExist", new object[0], null, new NamespaceImport[]
                    {
                        new NamespaceImport("DotVVM.Framework.Tests")
                    }));
            }
            catch (Exception x)
            {
                Assert.AreEqual(x.Message, "could not find static member NotExist on type DotVVM.Framework.Tests.Resource1");
            }
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

        class MoqComponent : DotvvmBindableObject
        {
            public object Property
            {
                get { return (object)GetValue(PropertyProperty); }
                set { SetValue(PropertyProperty, value); }
            }
            public static DotvvmProperty PropertyProperty;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BindingCompiler_PropertyRegisteredTwiceThrowException()
        {
            MoqComponent.PropertyProperty = DotvvmProperty.Register<object, MoqComponent>(t => t.Property);
            DotvvmProperty.Register<bool, MoqComponent>(t => t.Property);
        }

        [TestMethod]
        public void BindingCompiler_Valid_MethodCallOnValue()
        {
            var viewModel = new TestViewModel2 { MyProperty = 42 };
            Assert.AreEqual(ExecuteBinding("13.ToString()", viewModel), "13");
        }

        [TestMethod]
        public void BindingCompiler_Valid_IntStringConcat()
        {
            var viewModel = new TestViewModel { StringProp = "string", TestViewModel2 = new TestViewModel2 { MyProperty = 0 } };
            Assert.AreEqual(ExecuteBinding("StringProp + TestViewModel2.MyProperty", viewModel), "string0");
        }

        [TestMethod]
        public void BindingCompiler_Valid_EnumStringComparison()
        {
            var viewModel = new TestViewModel { EnumProperty = TestEnum.A };
            Assert.AreEqual(ExecuteBinding("EnumProperty == 'A'", viewModel), true);
            Assert.AreEqual(ExecuteBinding("EnumProperty == 'B'", viewModel), false);
        }

        [TestMethod]
        public void BindingCompiler_Valid_EnumStringComparison_Underscore()
        {
            var viewModel = new TestViewModel { EnumProperty = TestEnum.Underscore_hhh };
            Assert.AreEqual(ExecuteBinding("EnumProperty == 'Underscore_hhh'", viewModel), true);
            Assert.AreEqual(ExecuteBinding("EnumProperty == 'B'", viewModel), false);
        }

        [TestMethod]
        public void BindingCompiler_Valid_EnumMemberAccess()
        {
            var viewModel = new TestViewModel { EnumProperty = TestEnum.Underscore_hhh, StringProp = "abc" };
            Assert.AreEqual(ExecuteBinding("TestEnum.A", viewModel), TestEnum.A);
            Assert.AreEqual(ExecuteBinding("TestEnum.Underscore_hhh", viewModel), TestEnum.Underscore_hhh);
            Assert.AreEqual(ExecuteBinding("EnumProperty == TestEnum.Underscore_hhh", viewModel), true);
            Assert.AreEqual(ExecuteBinding("StringProp == 'abc' ? TestEnum.A : TestEnum.Underscore_hhh", viewModel), TestEnum.A);
            Assert.AreEqual(ExecuteBinding("StringProp == 'abcd' ? TestEnum.A : TestEnum.Underscore_hhh", viewModel), TestEnum.Underscore_hhh);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void BindingCompiler_Invalid_EnumStringComparison()
        {
            var viewModel = new TestViewModel { EnumProperty = TestEnum.A };
            ExecuteBinding("Enum == 'ghfjdskdjhbvdksdj'", viewModel);
        }

        [TestMethod]
        public void BindingCompiler_Valid_GenericMethodCall()
        {
            var viewModel = new TestViewModel();
            Assert.AreEqual(ExecuteBinding("GetType(Identity(42))", viewModel), typeof(int));
        }

        [TestMethod]
        public void BindingCompiler_Valid_DefaultParameterMethod()
        {
            var viewModel = new TestViewModel() { StringProp = "A" };
            Assert.AreEqual(ExecuteBinding("Cat(42)", viewModel), "42A");
        }


        [TestMethod]
        public void BindingCompiler_Valid_Char()
        {
            var viewModel = new TestViewModel() { };
            Assert.AreEqual(ExecuteBinding("GetCharCode('a')", viewModel), (int)'a');
        }

        [TestMethod]
        public void BindingCompiler_Valid_CollectionIndex()
        {
            var viewModel = new TestViewModel2() { Collection = new List<Something>() { new Something { Value = true } } };
            Assert.AreEqual(ExecuteBinding("Collection[0].Value ? 'a' : 'b'", viewModel), "a");
        }

        [TestMethod]
        public void BindingCompiler_Valid_AndAlso()
        {
            var viewModel = new TestViewModel() { };
            Assert.AreEqual(false, ExecuteBinding("false && BoolMethod()", viewModel));
            Assert.AreEqual(false, viewModel.BoolMethodExecuted);
        }

        [TestMethod]
        public void BindingCompiler_Valid_NullCoallescence()
        {
            var viewModel = new TestViewModel() { StringProp = "AHOJ 12" };
            Assert.AreEqual("AHOJ 12", ExecuteBinding("StringProp2 ?? (StringProp ?? 'HUHHHHE')", viewModel));
        }

        [TestMethod]
        public void BindingCompiler_Valid_ImportedStaticClass()
        {
            var result = ExecuteBinding($"TestStaticClass.GetSomeString()", new TestViewModel());
            Assert.AreEqual(result, TestStaticClass.GetSomeString());
        }

        [TestMethod]
        public void BindingCompiler_Valid_SystemStaticClass()
        {
            var result = ExecuteBinding($"String.Empty", new TestViewModel());
            Assert.AreEqual(result, String.Empty);
        }

        [TestMethod]
        public void BindingCompiler_Valid_StaticClassWithNamespace()
        {
            var result = ExecuteBinding($"System.Text.Encoding.ASCII.GetBytes('ahoj')", new TestViewModel());
            Assert.IsTrue(Encoding.ASCII.GetBytes("ahoj").SequenceEqual((byte[])result));
        }

        [TestMethod]
        public void BindingCompiler_Valid_MemberAssignment()
        {
            var vm = new TestViewModel() { TestViewModel2 = new TestViewModel2() };
            var result = ExecuteBinding($"TestViewModel2.SomeString = '42'", vm);
            Assert.AreEqual("42", vm.TestViewModel2.SomeString);
        }


        [TestMethod]
        public void BindingCompiler_Valid_NamespaceAlias()
        {
            var result = ExecuteBinding("Alias.TestClass2.Property", new object[0], null, new NamespaceImport[] { new NamespaceImport("DotVVM.Framework.Tests.Binding.TestNamespace2", "Alias") });
            Assert.AreEqual(TestNamespace2.TestClass2.Property, result);
        }

        [TestMethod]
        public void BindingCompiler_Valid_MultipleNamespaceAliases()
        {
            var result = ExecuteBinding("Alias.TestClass1.Property", new object[0], null, new NamespaceImport[] { new NamespaceImport("DotVVM.Framework.Tests.Binding.TestNamespace2", "Alias"), new NamespaceImport("DotVVM.Framework.Tests.Binding.TestNamespace1", "Alias") });
            Assert.AreEqual(TestNamespace1.TestClass1.Property, result);
        }

        [TestMethod]
        public void BindingCompiler_Valid_NamespaceImportAndAlias()
        {
            var result = ExecuteBinding("TestClass2.Property + Alias.TestClass1.Property", new object[0], null, new NamespaceImport[] { new NamespaceImport("DotVVM.Framework.Tests.Binding.TestNamespace2"), new NamespaceImport("DotVVM.Framework.Tests.Binding.TestNamespace1", "Alias") });
            Assert.AreEqual(TestNamespace2.TestClass2.Property + TestNamespace1.TestClass1.Property, result);
        }

		[TestMethod]
		public void BindingCompiler_Valid_TypeAlias()
		{
            var result = ExecuteBinding("Alias.Property", new object[0], null, 
				new NamespaceImport[] { new NamespaceImport("DotVVM.Framework.Tests.Binding.TestNamespace2.TestClass2", alias: "Alias") });
            Assert.AreEqual(TestNamespace2.TestClass2.Property, result);
		}


		[TestMethod]
		public void BindingCompiler_Valid_ToStringConstantConversion()
		{
			var result = ExecuteBinding("false", new object[0], null, expectedType: typeof(string));
			Assert.AreEqual("False", result);
		}


		[TestMethod]
		public void BindingCompiler_Valid_ToStringConversion()
		{
			var testViewModel = new TestViewModel();
			var result = ExecuteBinding("Identity(42)", new[] { testViewModel }, null, expectedType: typeof(string));
			Assert.AreEqual("42", result);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void BindingCompiler_InValid_ToStringConversion()
		{
			var testViewModel = new TestViewModel();
			var result = ExecuteBinding("_this", new[] { testViewModel }, null, expectedType: typeof(string));
		}
    }
    class TestViewModel
	{
		public string StringProp { get; set; }

		public TestViewModel2 TestViewModel2 { get; set; }
		public TestEnum EnumProperty { get; set; }
		public string StringProp2 { get; set; }

		public string SetStringProp(string a, int b)
		{
			var p = StringProp;
			StringProp = a + b;
			return p;
		}

		public T Identity<T>(T param)
			=> param;

		public Type GetType<T>(T param)
		{
			return typeof(T);
		}

		public string Cat<T>(T obj, string str = null)
		{
			return obj.ToString() + (str ?? StringProp);
		}

		public int GetCharCode(char ch)
			=> (int)ch;

		public bool BoolMethodExecuted { get; set; }
		public bool BoolMethod()
		{
			BoolMethodExecuted = true;
			return false;
		}
	}

	class TestViewModel2
	{
		public int MyProperty { get; set; }
		public string SomeString { get; set; }

		public IList<Something> Collection { get; set; }

		public override string ToString()
		{
			return SomeString + ": " + MyProperty;
		}
	}

	class Something
	{
		public bool Value { get; set; }
	}
	enum TestEnum
    {
        A,
        B,
        C,
        D,
        Underscore_hhh
    }


    public static class TestStaticClass
    {
        public static string GetSomeString() => "string 123";
    }
}
