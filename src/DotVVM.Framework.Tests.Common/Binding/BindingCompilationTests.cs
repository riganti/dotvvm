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
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Binding.Properties;
using System.Collections.Immutable;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Tests.Common;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class BindingCompilationTests
    {
        private DotvvmConfiguration configuration;
        private BindingCompilationService bindingService;

        [TestInitialize]
        public void INIT()
        {
            this.configuration = DotvvmTestHelper.DefaultConfig;
            this.bindingService = configuration.ServiceProvider.GetRequiredService<BindingCompilationService>();
        }

        public object ExecuteBinding(string expression, object[] contexts, DotvvmControl control, NamespaceImport[] imports = null, Type expectedType = null)
        {
            var context = DataContextStack.Create(contexts.FirstOrDefault()?.GetType() ?? typeof(object), extensionParameters: new[] {
                new CurrentMarkupControlExtensionParameter(new ResolvedTypeDescriptor(control?.GetType() ?? typeof(DotvvmControl)))
            });
            for (int i = 1; i < contexts.Length; i++)
            {
                context = DataContextStack.Create(contexts[i].GetType(), context);
            }
            Array.Reverse(contexts);
            var binding = new ResourceBindingExpression(bindingService, new object[] {
                context,
                new OriginalStringBindingProperty(expression),
                new BindingParserOptions(typeof(ResourceBindingExpression), importNamespaces: imports?.ToImmutableList()),
                new ExpectedTypeBindingProperty(expectedType ?? typeof(object))
            });
            return binding.BindingDelegate.Invoke(contexts, control);
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
                Assert.IsTrue(x.AllInnerExceptions().Any(e => e.Message.Contains("Could not find static member NotExist on type DotVVM.Framework.Tests.Resource1.")));
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

        [TestMethod]
        public void BindingCompiler_Valid_Lambda_Test()
        {
            var viewModel = new TestViewModel { StringProp = "abc" };
            var function1 = (Func<string, string>)ExecuteBinding("(string arg) => SetStringProp(arg, 123)", viewModel);
            var result1 = function1("test");
            Assert.AreEqual("abc", result1);
            Assert.AreEqual("test123", viewModel.StringProp);

            var function2 = (Func<char, int>)ExecuteBinding("(char arg) => GetCharCode(arg)", viewModel);
            var result2 = function2('A');
            Assert.AreEqual(65, result2);
        }

        [TestMethod]
        [DataRow("() => ;", typeof(Action), null)]
        [DataRow("() => \"HelloWorld\"", typeof(Func<string>), typeof(string))]
        [DataRow("() => 11", typeof(Func<int>), typeof(int))]
        [DataRow("() => 4.5f", typeof(Func<float>), typeof(float))]
        [DataRow("() => 4.5", typeof(Func<double>), typeof(double))]
        [DataRow("() => 11 + 4.5", typeof(Func<double>), typeof(double))]
        public void BindingCompiler_Valid_LambdaReturnType(string expr, Type lambdaType, Type returnType)
        {
            var viewModel = new TestViewModel();
            var binding = ExecuteBinding(expr, viewModel);
            Assert.IsTrue(binding is Delegate);
            Assert.AreEqual(lambdaType, binding.GetType());        
            Assert.AreEqual(returnType, ((Delegate)binding).DynamicInvoke()?.GetType());
        }

        [TestMethod]
        [DataRow("(int arg) => ;", typeof(int))]
        [DataRow("(string arg1, float arg2) => ;", typeof(string), typeof(float))]
        [DataRow("(System.Collections.Generic.List<int> arg) => ;", typeof(List<int>))]
        public void BindingCompiler_Valid_LambdaParameterType(string expr, params Type[] parameterTypes)
        {
            var viewModel = new TestViewModel();
            var binding = ExecuteBinding(expr, viewModel);
            Assert.IsTrue(binding is Delegate);
            var generics = ((Delegate)binding).GetType().GenericTypeArguments;

            var index = 0;
            foreach (var paramType in generics)
                Assert.AreEqual(parameterTypes[index++], paramType);
        }

        [TestMethod]
        [DataRow("(int arg, float arg) => ;", DisplayName = "Can not use same identifier for multiple parameters")]
        [DataRow("(object _this) => ;", DisplayName = "Can not use already used identifiers for parameters")]
        public void BindingCompiler_Invalid_LambdaParameters(string expr)
        {
            var viewModel = new TestViewModel();
            Assert.ThrowsException<AggregateException>(() => ExecuteBinding(expr, viewModel));         
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
        public void BindingCompiler_PropertyRegisteredTwiceThrowException()
        {
            Assert.ThrowsException<ArgumentException>(() => {
                MoqComponent.PropertyProperty = DotvvmProperty.Register<object, MoqComponent>(t => t.Property);
                DotvvmProperty.Register<bool, MoqComponent>(t => t.Property);
            });
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
        public void BindingCompiler_Invalid_EnumStringComparison()
        {
            Assert.ThrowsException<AggregateException>(() => {
                var viewModel = new TestViewModel { EnumProperty = TestEnum.A };
                ExecuteBinding("Enum == 'ghfjdskdjhbvdksdj'", viewModel);
            });
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
        public void BindingCompiler_Valid_CollectionCount()
        {
            var viewModel = new TestViewModel2() { Collection = new List<Something>() { new Something { Value = true } } };
            Assert.AreEqual(ExecuteBinding("Collection.Count > 0", viewModel), true);
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
        public void BindingCompiler_Invalid_ToStringConversion()
        {
            Assert.ThrowsException<AggregateException>(() => {
                var testViewModel = new TestViewModel();
                var result = ExecuteBinding("_this", new[] { testViewModel }, null, expectedType: typeof(string));
            });
        }

        [TestMethod]
        public void BindingCompiler_NullConversion()
        {
            var testViewModel = new TestViewModel { StringProp = "ahoj" };
            ExecuteBinding("StringProp = null", testViewModel);
            Assert.AreEqual(null, testViewModel.StringProp);
        }

        [TestMethod]
        public void BindingCompiler_Parents()
        {
            var result = ExecuteBinding("_this.StringProp + _parent.StringProp + StringProp + _parent0.StringProp + _parent1.StringProp + _parent2.StringProp + _parent3.StringProp + _parent4.StringProp", new[] { new TestViewModel { StringProp = "1" },
                new TestViewModel { StringProp = "2" },
                new TestViewModel { StringProp = "3" },
                new TestViewModel { StringProp = "4" },
                new TestViewModel { StringProp = "5" }});
            Assert.AreEqual("54554321", result);
        }

        [TestMethod]
        public void BindingCompiler_NullableDateExpression()
        {
            bool execute(TestViewModel viewModel)
            {
                var result = ExecuteBinding("DateFrom == null || DateTo == null || DateFrom.Value <= DateTo.Value", viewModel);
                var result2 = ExecuteBinding("DateFrom == null || DateTo == null || DateFrom <= DateTo", viewModel);
                Assert.IsInstanceOfType(result, typeof(bool));
                Assert.AreEqual(result, result2);
                return (bool)result;
            }

            Assert.AreEqual(true, execute(new TestViewModel { DateFrom = null, DateTo = new DateTime(5) }));
            Assert.AreEqual(true, execute(new TestViewModel { DateTo = new DateTime(5), DateFrom = null }));
            Assert.AreEqual(true, execute(new TestViewModel { DateFrom = new DateTime(0), DateTo = new DateTime(5) }));
            Assert.AreEqual(false, execute(new TestViewModel { DateFrom = new DateTime(5), DateTo = new DateTime(0) }));
        }


        [TestMethod]
        public void BindingCompiler_ImplicitConstantConversionInsideConditional()
        {
            var result = ExecuteBinding("true ? 'Utc' : 'Local'", new object[] { }, null, null, typeof(DateTimeKind));
            Assert.AreEqual(DateTimeKind.Utc, result);
        }

        [TestMethod]
        public void BindingCompiler_ImplicitConversion_ConditionalLongAndInt()
        {
            var result = ExecuteBinding("_this != null ? _this.LongProperty : 0", new [] { new TestViewModel { LongProperty = 5 } });
            Assert.AreEqual(5L, result);
        }

        [TestMethod]
        public void BindingCompiler_SimpleBlockExpression()
        {
            var result = ExecuteBinding("SetStringProp2(StringProp + 'kk'); StringProp = StringProp2 + 'll'", new [] { new TestViewModel { StringProp = "a" } });
            Assert.AreEqual("akkll", result);
        }

        [TestMethod]
        public void BindingCompiler_SimpleBlockExpression_TaskSequence_TaskNonTask()
        {
            var vm = new TestViewModel4();
            var resultTask = (Task)ExecuteBinding("Increment(); Number = Number * 5", new[] { vm });
            resultTask.Wait();
            Assert.AreEqual(5, vm.Number);
        }

        [TestMethod]
        public void BindingCompiler_SimpleBlockExpression_TaskSequence_NonTaskTask()
        {
            var vm = new TestViewModel4();
            var resultTask = (Task)ExecuteBinding("Number = 10; Increment();", new[] { vm });
            resultTask.Wait();
            Assert.AreEqual(11, vm.Number);
        }

        [TestMethod]
        public void BindingCompiler_SimpleBlockExpression_TaskSequence_VoidTaskJoining()
        {
            var vm = new TestViewModel4();
            var resultTask = (Task)ExecuteBinding("Increment(); Multiply()", new[] { vm });
            resultTask.Wait();
            Assert.AreEqual(10, vm.Number);
        }


        [TestMethod]
        public void BindingCompiler_MultiBlockExpression()
        {
            TestViewModel vm = new TestViewModel { StringProp = "a" };
            var result = ExecuteBinding("StringProp = StringProp + 'll'; SetStringProp2(StringProp + 'kk'); StringProp = 'nn'; StringProp2 + '|' + StringProp", new [] { vm });
            Assert.AreEqual("nn", vm.StringProp);
            Assert.AreEqual("allkk", vm.StringProp2);
            Assert.AreEqual("allkk|nn", result);
        }

        [TestMethod]
        [ExpectedExceptionMessageSubstring(typeof(AggregateException), "Identifier name")]
        public void BindingCompiler_MemberAccessIdentifierMissing_Throws()
        {
            TestViewModel vm = new TestViewModel { StringProp = "a" };
            var result = ExecuteBinding("StringProp.", new[] { vm });
        }

        [TestMethod]
        public void BindingCompiler_MultiBlockExpression_EnumAtEnd_CorrectResult()
        {
            TestViewModel vm = new TestViewModel { StringProp = "a" };
            var result = ExecuteBinding("StringProp = StringProp + 'll'; IntProp = MethodWithOverloads(); GetEnum()", new[] { vm });

            Assert.IsInstanceOfType(result, typeof(TestEnum));
            Assert.AreEqual(TestEnum.A, (TestEnum)result);
        }

        [TestMethod]
        [ExpectedExceptionMessageSubstring(typeof(AggregateException), "Could not implicitly convert expression of type System.Void to System.Object")]
        public void BindingCompiler_MultiBlockExpression_EmptyBlockAtEnd_Throws()
        {
            TestViewModel vm = new TestViewModel { StringProp = "a" };
            var result = ExecuteBinding("GetEnum();", new[] { vm });
        }

        [TestMethod]
        [ExpectedExceptionMessageSubstring(typeof(AggregateException), "Could not implicitly convert expression of type System.Void to System.Object")]
        public void BindingCompiler_MultiBlockExpression_WhitespaceBlockAtEnd_Throws()
        {
            TestViewModel vm = new TestViewModel { StringProp = "a" };
            var result = ExecuteBinding("GetEnum(); ", new[] { vm });
        }

        [TestMethod]
        public void BindingCompiler_MultiBlockExpression_EmptyBlockInTheMiddle_Throws()
        {
            TestViewModel vm = new TestViewModel { StringProp = "a" };
            var result = ExecuteBinding("StringProp = StringProp; ; MethodWithOverloads()", new[] { vm });

            Assert.IsInstanceOfType(result, typeof(int));
            Assert.AreEqual(1, (int)result);
        }

        [TestMethod]
        public void BindingCompiler_MultiBlockExpression_EmptyBlockAtStart_Throws()
        {
            TestViewModel vm = new TestViewModel { StringProp = "a" };
            var result = ExecuteBinding("; MethodWithOverloads()", new[] { vm });

            Assert.IsInstanceOfType(result, typeof(int));
            Assert.AreEqual(1, (int)result);
        }

        [TestMethod]
        public void BindingCompiler_MultiBlockExpression_AssigmentAtEnd_CorrectResult()
        {
            TestViewModel vm = new TestViewModel { StringProp = "a" };
            var result = ExecuteBinding("StringProp = StringProp + 'll'; IntProp = MethodWithOverloads()", new[] { vm });

            Assert.IsInstanceOfType(result, typeof(int));
            Assert.AreEqual(1, (int)result);
        }

        [TestMethod]
        public void BindingCompiler_DelegateConversion_TaskFromResult()
        {
            TestViewModel vm = new TestViewModel { StringProp = "a" };
            var function = ExecuteBinding("StringProp + arg", new [] { vm }, null, expectedType: typeof(Func<string, Task<string>>)) as Func<string, Task<string>>;
            Assert.IsNotNull(function);
            var result = function("test");
            Assert.IsTrue(result.IsCompleted);
            Assert.AreEqual("atest", result.Result);
        }

        [TestMethod]
        public void BindingCompiler_DelegateConversion_CompletedTask()
        {
            TestViewModel vm = new TestViewModel { StringProp = "a" };
            var function = ExecuteBinding("SetStringProp(arg, 4)", new [] { vm }, null, expectedType: typeof(Func<string, Task>)) as Func<string, Task>;
            Assert.IsNotNull(function);
            var result = function("test");
            Assert.IsTrue(result.IsCompleted);
            Assert.AreEqual("test4", vm.StringProp);
        }

        [TestMethod]
        public void BindingCompiler_DelegateFromMethodGroup()
        {
            var result = ExecuteBinding("_this.MethodWithOverloads", new [] { new TestViewModel() }, null, expectedType: typeof(Func<int, int>)) as Func<int, int>;

            Assert.IsNotNull(result);
            Assert.AreEqual(42, result(42));
        }

        [TestMethod]
        public void BindingCompiler_ComparisonOperators()
        {
            var result = ExecuteBinding("LongProperty < TestViewModel2.MyProperty && LongProperty > TestViewModel2.MyProperty", new [] { new TestViewModel { TestViewModel2 = new TestViewModel2() } });
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void BindingCompiler_Errors_AssigningToType()
        {
            var aggEx = Assert.ThrowsException<AggregateException>(() => ExecuteBinding("System.String = 123", new [] { new TestViewModel() }));
            var ex = aggEx.AllInnerExceptions().Single(e => e.InnerException == null);
            Assert.IsTrue(ex.Message.Contains("Expression must be writeable"));
        }
    }
    class TestViewModel
    {
        public string StringProp { get; set; }
        public int IntProp { get; set; }
        public TestViewModel2 TestViewModel2 { get; set; }
        public TestViewModel2 TestViewModel2B { get; set; }
        public TestEnum EnumProperty { get; set; }
        public string StringProp2 { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public object Time { get; set; } = TimeSpan.FromSeconds(5);
        public Guid GuidProp { get; set; }

        public long LongProperty { get; set; }

        public long[] LongArray => new long[] { 1, 2, long.MaxValue };
        public TestViewModel2[] VmArray => new TestViewModel2[] { new TestViewModel2() };
        public int[] IntArray { get; set; }

        public string SetStringProp(string a, int b)
        {
            var p = StringProp;
            StringProp = a + b;
            return p;
        }

        public TestEnum GetEnum() {
            return TestEnum.A;
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

        public void SetStringProp2(string value)
        {
            this.StringProp2 = value;
        }

        public async Task<string> GetStringPropAsync()
        {
            await Task.Delay(10);
            return StringProp;
        }

        public int MethodWithOverloads() => 1;
        public int MethodWithOverloads(int i) => i;
        public string MethodWithOverloads(string i) => i;
        public string MethodWithOverloads(DateTime i) => i.ToString();
        public int MethodWithOverloads(int a, int b) => a + b;
    }

    class TestViewModel2
    {
        public int MyProperty { get; set; }
        public string SomeString { get; set; }
        public DateTime NonNullableDate { get; set; }
        public DateTime? NullableDate { get; set; }
        public TestEnum Enum { get; set; }
        public TestEnum? NullableEnum { get; set; }
        public IList<Something> Collection { get; set; }
        public TestViewModel3 ChildObject { get; set; }
        public TestStruct Struct { get; set; }
        public override string ToString()
        {
            return SomeString + ": " + MyProperty;
        }
    }
    class TestViewModel3 : DotvvmViewModelBase
    {
        public string SomeString { get; set; }
    }

    class TestViewModel4
    {
        public int Number { get; set; }

        public async Task Increment()
        {
            await Task.Delay(100);
            Number += 1;
        }

        public Task Multiply()
        {
            Number *= 10;
            return Task.Delay(100);
        }
    }

    struct TestStruct
    {
        public int Int { get; set; }
    }
    class Something
    {
        public bool Value { get; set; }
        public string StringValue { get; set; }
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
