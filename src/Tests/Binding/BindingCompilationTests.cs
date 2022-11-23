﻿using DotVVM.Framework.Binding;
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
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Testing;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class BindingCompilationTests
    {
        private DotvvmConfiguration configuration;
        private BindingCompilationService bindingService;

        [TestInitialize]
        public void Init()
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
            return ExecuteBinding(expression, context, contexts, control, imports, expectedType);
        }
        public object ExecuteBinding(string expression, DataContextStack contextType, object[] contexts, DotvvmControl control, NamespaceImport[] imports = null, Type expectedType = null)
        {
            Array.Reverse(contexts);
            var binding = new ResourceBindingExpression(bindingService, new object[] {
                contextType,
                new OriginalStringBindingProperty(expression),
                BindingParserOptions.Resource.AddImports(imports),
                new ExpectedTypeBindingProperty(expectedType ?? typeof(object))
            });
            return binding.BindingDelegate.Invoke(contexts, control);
        }

        public object ExecuteBinding(string expression, params object[] contexts)
        {
            return ExecuteBinding(expression, contexts, null);
        }

        public object ExecuteBinding(string expression, NamespaceImport[] imports, params object[] contexts)
        {
            return ExecuteBinding(expression, contexts, null, imports);
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
        [DataRow(@"$'Non-Interpolated'", "Non-Interpolated")]
        [DataRow(@"$'Non-Interpolated {{'", "Non-Interpolated {")]
        [DataRow(@"$'Non-Interpolated {{ no-expression }}'", "Non-Interpolated { no-expression }")]
        public void BindingCompiler_Valid_InterpolatedString_NoExpressions(string expression, string evaluated)
        {
            var binding = ExecuteBinding(expression);
            Assert.AreEqual(evaluated, binding);
        }

        [TestMethod]
        [DataRow(@"$'} Malformed'", "Unexpected token '$' ---->}<----")]
        [DataRow(@"$'{ Malformed'", "Could not find matching closing character '}' for an interpolated expression")]
        [DataRow(@"$'Malformed {expr'", "Could not find matching closing character '}' for an interpolated expression")]
        [DataRow(@"$'Malformed expr}'", "Unexpected token '$'Malformed expr ---->}<---- ")]
        [DataRow(@"$'Malformed {'", "Could not find matching closing character '}' for an interpolated expression")]
        [DataRow(@"$'Malformed }'", "Unexpected token '$'Malformed  ---->}<----")]
        [DataRow(@"$'Malformed {}'", "Expected expression, but instead found empty")]
        [DataRow(@"$'Malformed {StringProp; IntProp}'", "Expected end of interpolated expression, but instead found Semicolon")]
        [DataRow(@"$'Malformed {(string arg) => arg.Length}'", "Expected end of interpolated expression, but instead found Identifier")]
        [DataRow(@"$'Malformed {(StringProp == null) ? 'StringPropWasNull' : 'StringPropWasNotNull'}'", "Conditional expression needs to be enclosed in parentheses")]
        public void BindingCompiler_Invalid_InterpolatedString_MalformedExpression(string expression, string errorMessage)
        {
            try
            {
                ExecuteBinding(expression);
            }
            catch (Exception e)
            {
                // Get inner-most exception
                var current = e;
                while (current.InnerException != null)
                    current = current.InnerException;

                Assert.AreEqual(typeof(BindingCompilationException), current.GetType());
                StringAssert.Contains(current.Message, errorMessage);
            }
        }

        [TestMethod]
        [DataRow(@"$""Interpolated {StringProp} {StringProp}""", "Interpolated abc abc")]
        [DataRow(@"$'Interpolated {StringProp} {StringProp}'", "Interpolated abc abc")]
        [DataRow(@"$'Interpolated {StringProp.Length}'", "Interpolated 3")]
        public void BindingCompiler_Valid_InterpolatedString_WithSimpleExpressions(string expression, string evaluated)
        {
            var viewModel = new TestViewModel() { StringProp = "abc" };
            var binding = ExecuteBinding(expression, viewModel);
            Assert.AreEqual(evaluated, binding);
        }

        [TestMethod]
        [DataRow(@"$'{string.Join(', ', IntArray)}'", "1, 2, 3")]
        [DataRow(@"$'{string.Join(', ', 'abc', 'def', $'{string.Join(', ', IntArray)}')}'", "abc, def, 1, 2, 3")]
        public void BindingCompiler_Valid_InterpolatedString_NestedExpressions(string expression, string evaluated)
        {
            var viewModel = new TestViewModel { IntArray = new[] { 1, 2, 3 } };
            var binding = ExecuteBinding(expression, viewModel);
            Assert.AreEqual(evaluated, binding);
        }

        [TestMethod]
        public void BindingCompiler_Valid_AssignNullables()
        {
            var viewModel = new TestViewModel() { DateTime = DateTime.Now };
            ExecuteBinding("DateFrom = DateTime", viewModel);
            Assert.AreEqual(viewModel.DateTime, viewModel.DateFrom.Value);
        }

        [TestMethod]
        [DataRow(@"$'Interpolated {IntProp < LongProperty}'", "Interpolated True")]
        [DataRow(@"$'Interpolated {StringProp ?? 'StringPropWasNull'}'", "Interpolated StringPropWasNull")]
        [DataRow(@"$'Interpolated {((StringProp == null) ? 'StringPropWasNull' : 'StringPropWasNotNull')}'", "Interpolated StringPropWasNull")]
        public void BindingCompiler_Valid_InterpolatedString_WithComplexExpressions(string expression, string evaluated)
        {
            var viewModel = new TestViewModel() { IntProp = 1, LongProperty = 2 };
            var binding = ExecuteBinding(expression, viewModel);
            Assert.AreEqual(evaluated, binding);
        }

        [TestMethod]
        [DataRow(@"$'Interpolated {DateFrom:R}'", "Interpolated Fri, 11 Nov 2011 11:11:11 GMT")]
        [DataRow(@"$'Interpolated {$'{DateFrom:R}'}'", "Interpolated Fri, 11 Nov 2011 11:11:11 GMT")]
        [DataRow(@"$'Interpolated {$'{IntProp:0000}'}'", "Interpolated 0006")]
        public void BindingCompiler_Valid_InterpolatedString_WithFormattingComponent(string expression, string evaluated)
        {
            var viewModel = new TestViewModel() {
                DateFrom = DateTimeOffset.Parse("2011-11-11T11:11:11+00:00").UtcDateTime,
                IntProp = 6
            };
            var binding = ExecuteBinding(expression, viewModel);
            Assert.AreEqual(evaluated, binding);
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
        public void BindingCompiler_GenericMethodCall_ExplicitTypeParameters()
        {
            var viewModel = new TestViewModel { StringProp = "abc" };
            var result = (Type)ExecuteBinding("GetType<string>(StringProp)", viewModel);
            Assert.AreEqual(typeof(string), result);
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
        [DataRow("GetFirstGenericArgType(Tuple)", typeof(int))]
        [DataRow("Enumerable.Where(LongArray, item => item % 2 == 0)", typeof(long))]
        [DataRow("Enumerable.Select(LongArray, item => -item)", typeof(long), typeof(long))]
        [DataRow("Enumerable.Select(Enumerable.Where(LongArray, item => item % 2 == 0), item => -item)", typeof(long), typeof(long))]
        public void BindingCompiler_RegularGenericMethodsInference(string expr, params Type[] instantiations)
        {
            var viewModel = new TestViewModel() { StringProp = "abc" };
            var binding = ExecuteBinding(expr, new[] { viewModel }, null, new[] { new NamespaceImport("System.Linq") });
            var genericArgs = binding.GetType().GetGenericArguments();

            for (var argIndex = 0; argIndex < genericArgs.Length; argIndex++)
                Assert.AreEqual(instantiations[argIndex], genericArgs[argIndex]);
        }

        [TestMethod]
        [DataRow("LongArray.Where(item => item % 2 == 0)", typeof(long))]
        [DataRow("LongArray.Select(item => -item)", typeof(long), typeof(long))]
        [DataRow("LongArray.Where(item => item % 2 == 0).Select(item => -item)", typeof(long), typeof(long))]
        public void BindingCompiler_ExtensionGenericMethodsInference(string expr, params Type[] instantiations)
        {
            var viewModel = new TestViewModel() { StringProp = "abc" };
            var binding = ExecuteBinding(expr, new[] { viewModel }, null, new[] { new NamespaceImport("System.Linq") });
            var genericArgs = binding.GetType().GetGenericArguments();

            for (var argIndex = 0; argIndex < genericArgs.Length; argIndex++)
                Assert.AreEqual(instantiations[argIndex], genericArgs[argIndex]);
        }

        [TestMethod]
        [DataRow("LongArray.All(item => item % 2 == 0)", typeof(bool))]
        [DataRow("LongArray.Any(item => item % 2 == 0)", typeof(bool))]
        [DataRow("LongArray.Concat(LongArray).ToArray()", typeof(long[]))]
        [DataRow("LongArray.FirstOrDefault(item => item % 2 == 0)", typeof(long))]
        [DataRow("LongArray.LastOrDefault(item => item % 2 == 0)", typeof(long))]
        [DataRow("LongArray.Max(item => -item)", typeof(long))]
        [DataRow("LongArray.Min(item => -item)", typeof(long))]
        [DataRow("LongArray.OrderBy(item => item).ToArray()", typeof(long[]))]
        [DataRow("LongArray.OrderByDescending(item => item).ToArray()", typeof(long[]))]
        [DataRow("LongArray.Select(item => -item).ToArray()", typeof(long[]))]
        [DataRow("LongArray.Where(item => item % 2 == 0).ToArray()", typeof(long[]))]
        public void BindingCompiler_LinqMethodsInference(string expr, Type resultType)
        {
            var viewModel = new TestViewModel() { StringProp = "abc" };
            var result = ExecuteBinding(expr, new[] { viewModel }, null, new[] { new NamespaceImport("System.Linq") });
            Assert.AreEqual(resultType, result.GetType());
        }

        [TestMethod]
        [DataRow("List.RemoveFirst(item => item == 2)", new[] { 1, 3 })]
        [DataRow("List.RemoveLast(item => item == 2)", new[] { 1, 3 })]
        [DataRow("List.AddOrUpdate(11, i => i == 2, i => 22)", new[] { 1, 22, 3 })]
        [DataRow("List.AddOrUpdate(11, i => i == 22, i => 33)", new[] { 1, 2, 3, 11 })]
        public void BindingCompiler_MoreComplexInference(string expr, int[] result)
        {
            var viewModel = new TestViewModel() { StringProp = "abc", List = new List<int>() { 1, 2, 3 } };
            ExecuteBinding(expr, new[] { viewModel }, null, new[] { new NamespaceImport("DotVVM.Framework.Binding.HelperNamespace") }, expectedType: typeof(void));
            CollectionAssert.AreEqual(result, viewModel.List);
        }

        [TestMethod]
        [DataRow("(int arg, float arg) => ;", DisplayName = "Cannot use same identifier for multiple parameters")]
        [DataRow("(object _this) => ;", DisplayName = "Cannot use already used identifiers for parameters")]
        public void BindingCompiler_Invalid_LambdaParameters(string expr)
        {
            var viewModel = new TestViewModel();
            Assert.ThrowsException<BindingPropertyException>(() => ExecuteBinding(expr, viewModel));
        }

        [TestMethod]
        [DataRow("(TestViewModel vm) => vm.IntProp = 11")]
        [DataRow("(TestViewModel vm) => vm.GetEnum()")]
        [DataRow("(TestViewModel vm) => ;")]
        public void BindingCompiler_Valid_LambdaToAction(string expr)
        {
            var viewModel = new TestViewModel();
            var binding = ExecuteBinding(expr, new[] { viewModel }, null, expectedType: typeof(Action<TestViewModel>)) as Action<TestViewModel>;
            Assert.AreEqual(typeof(Action<TestViewModel>), binding.GetType());
            binding.Invoke(viewModel);
        }

        [TestMethod]
        [DataRow("List.RemoveAll(item => item % 2 != 0)")]
        public void BindingCompiler_Valid_LambdaToPredicate(string expr)
        {
            var viewModel = new TestViewModel() { List = new List<int>() { 1, 2, 3 } };
            var removedCount = ExecuteBinding(expr, new[] { viewModel }, null, expectedType: typeof(int));
            Assert.AreEqual(2, removedCount);
            CollectionAssert.AreEqual(new List<int> { 2 }, viewModel.List);
        }

        [TestMethod]
        [DataRow("ActionInvoker(arg => StringProp = arg)")]
        [DataRow("ActionInvoker(arg => StringProp = ActionInvoker(innerArg => StringProp = innerArg))")]
        [DataRow("Action2Invoker((arg1, arg2) => StringProp = arg1 + arg2)")]

        public void BindingCompiler_Valid_ParameterLambdaToAction(string expr)
        {
            var viewModel = new TestLambdaCompilation();
            var result = ExecuteBinding(expr, viewModel);
            Assert.AreEqual("Action", result);
        }

        [TestMethod]
        [DataRow("DelegateInvoker(arg => StringProp = arg)")]
        [DataRow("DelegateInvoker(arg => arg + arg)")]
        public void BindingCompiler_Valid_LambdaParameter_PreferFunc(string expr)
        {
            var viewModel = new TestLambdaCompilation();
            var result = ExecuteBinding(expr, viewModel);
            Assert.AreEqual("Func", result);
        }

        [TestMethod]
        [DataRow("(int? arg) => arg.Value + 1", typeof(Func<int?, int>))]
        [DataRow("(double? arg) => arg.Value + 0.1", typeof(Func<double?, double>))]
        public void BindingCompiler_Valid_LambdaParameter_Nullable(string expr, Type type)
        {
            var viewModel = new TestLambdaCompilation();
            var result = ExecuteBinding(expr, viewModel);
            Assert.AreEqual(type, result.GetType());
        }

        [TestMethod]
        [DataRow("(int[] array) => array[0]", typeof(Func<int[], int>))]
        [DataRow("(double[] array) => array[0]", typeof(Func<double[], double>))]
        [DataRow("(int[][] jaggedArray) => jaggedArray[0][1]", typeof(Func<int[][], int>))]
        [DataRow("(int[][] jaggedArray) => jaggedArray[0]", typeof(Func<int[][], int[]>))]
        public void BindingCompiler_Valid_LambdaParameter_Array(string expr, Type type)
        {
            var viewModel = new TestLambdaCompilation();
            var result = ExecuteBinding(expr, viewModel);
            Assert.AreEqual(type, result.GetType());
        }

        [TestMethod]
        [DataRow("(int?[] arrayOfNullables) => arrayOfNullables[0]", typeof(Func<int?[], int?>))]
        [DataRow("(System.Collections.Generic.List<int?> list) => list[0]", typeof(Func<List<int?>, int?>))]
        [DataRow("(System.Collections.Generic.List<int?[]> list) => list[0]", typeof(Func<List<int?[]>, int?[]>))]
        [DataRow("(System.Collections.Generic.List<int?[][]> list) => list[0][0]", typeof(Func<List<int?[][]>, int?[]>))]
        [DataRow("(System.Collections.Generic.Dictionary<int?,double?> dict) => dict[0]", typeof(Func<Dictionary<int?, double?>, double?>))]
        [DataRow("(System.Collections.Generic.Dictionary<int?[],double?> dict) => 0", typeof(Func<Dictionary<int?[], double?>, int>))]
        public void BindingCompiler_Valid_LambdaParameter_CombinedTypeModifies(string expr, Type type)
        {
            var viewModel = new TestLambdaCompilation();
            var result = ExecuteBinding(expr, viewModel);
            Assert.AreEqual(type, result.GetType());
        }

        [TestMethod]
        [DataRow("(string? arg) => arg")]
        [DataRow("(int[]? arg) => arg")]
        public void BindingCompiler_Invalid_LambdaParameter_NullableReferenceTypes(string expr)
        {
            var exceptionThrown = false;
            try
            {
                var viewModel = new TestLambdaCompilation();
                ExecuteBinding(expr, viewModel);
            }
            catch (Exception e)
            {
                // Get inner-most exception
                var current = e;
                while (current.InnerException != null)
                    current = current.InnerException;

                Assert.AreEqual(typeof(BindingCompilationException), current.GetType());
                StringAssert.Contains(current.Message, "as nullable is not supported!");
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public void BindingCompiler_Valid_ExtensionMethods()
        {
            var viewModel = new TestViewModel();
            var result = (long[])ExecuteBinding("LongArray.Where((long item) => item % 2 != 0).ToArray()", new[] { new NamespaceImport("System.Linq") }, viewModel);
            CollectionAssert.AreEqual(viewModel.LongArray.Where(item => item % 2 != 0).ToArray(), result);
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
        public void BindingCompiler_EnumToStringConversion()
        {
            var result1 = ExecuteBinding("DotVVM.Framework.Tests.Binding.TestEnum.Underscore_hhh", new object[0], null, expectedType: typeof(string));
            Assert.AreEqual("Underscore_hhh", result1);
            var result2 = ExecuteBinding("DotVVM.Framework.Tests.Binding.TestEnum.SpecialField", new object[0], null, expectedType: typeof(string));
            Assert.AreEqual("xxx", result2);
            var result3 = ExecuteBinding("EnumProperty", new object[] { new TestViewModel { EnumProperty = TestEnum.A } }, null, expectedType: typeof(string));
            Assert.AreEqual("A", result3);
            var result4 = ExecuteBinding("EnumProperty", new object[] { new TestViewModel { EnumProperty = TestEnum.SpecialField } }, null, expectedType: typeof(string));
            Assert.AreEqual("xxx", result4);
        }

        [TestMethod]
        public void BindingCompiler_Invalid_EnumStringComparison()
        {
            Assert.ThrowsException<BindingPropertyException>(() => {
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
        public void BindingCompiler_Valid_NullCoalescence()
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
        public void BindingCompiler_Valid_MemberAssignmentWithUnaryMinus()
        {
            var vm = new TestViewModel();
            var result = ExecuteBinding($"IntProp=-42", vm);
            Assert.AreEqual(-42, vm.IntProp);
        }

        [TestMethod]
        public void BindingCompiler_Valid_MemberAssignmentWithUnaryNegation()
        {
            var vm = new TestViewModel();
            var result = ExecuteBinding($"true==!false", vm);
            Assert.IsTrue((bool)result);
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
        [DataRow("_index", 21)]
        [DataRow("_parent._index", 10)]
        [DataRow("_root._index", 10)]
        [DataRow("_parent0._index", 21)]
        [DataRow("_this._collection.IsOdd", true)]
        [DataRow("_parent._collection.IsOdd", false)]
        public void BindingCompiler_IndexExtensionParameter(string expr, object expectedResult)
        {
            var dc1 = DataContextStack.Create(
                typeof(string),
                extensionParameters: new BindingExtensionParameter[] {
                    new CurrentCollectionIndexExtensionParameter(),
                    new BindingCollectionInfoExtensionParameter("_collection")
                });
            var dc2 = DataContextStack.Create(
                typeof(string),
                parent: dc1,
                extensionParameters: new BindingExtensionParameter[] {
                    new CurrentCollectionIndexExtensionParameter(),
                    new BindingCollectionInfoExtensionParameter("_collection")
                });

            var control1 = new DataItemContainer() { DataItemIndex = 10 };
            control1.SetDataContextType(dc1);
            var control2 = new DataItemContainer() { DataItemIndex = 21 };
            control2.SetDataContextType(dc2);
            control1.Children.Add(control2);
            var html = new HtmlGenericControl("span");
            control2.Children.Add(html);

            var result = ExecuteBinding(expr, dc2, new object[] { "a", "b" }, html);
            Assert.AreEqual(expectedResult, result);
            var result2 = ExecuteBinding(expr, dc2, new object[] { "a", "b" }, control2);
            Assert.AreEqual(expectedResult, result2);
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
            Assert.ThrowsException<BindingPropertyException>(() => {
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
        [ExpectedExceptionMessageSubstring(typeof(BindingPropertyException), "Identifier name")]
        public void BindingCompiler_MemberAccessIdentifierMissing_Throws()
        {
            TestViewModel vm = new TestViewModel { StringProp = "a" };
            var result = ExecuteBinding("StringProp.", new[] { vm });
        }

        [TestMethod]
        public void BindingCompiler_DictionaryIndexer_Get()
        {
            TestViewModel5 vm = new TestViewModel5();
            var result = ExecuteBinding("Dictionary[2]", new[] { vm });
            Assert.AreEqual(22, result);
        }

        [TestMethod]
        public void BindingCompiler_DictionaryIndexer_Set()
        {
            TestViewModel5 vm = new TestViewModel5();
            ExecuteBinding("Dictionary[1] = 123", new[] { vm }, null, expectedType: typeof(void));
            Assert.AreEqual(123, vm.Dictionary[1]);
        }

        [TestMethod]
        public void BindingCompiler_ListIndexer_Get()
        {
            TestViewModel5 vm = new TestViewModel5();
            var result = ExecuteBinding("List[1]", new[] { vm });
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void BindingCompiler_ListIndexer_Set()
        {
            TestViewModel5 vm = new TestViewModel5();
            ExecuteBinding("List[1] = 111", new[] { vm }, null, expectedType: typeof(void));
            Assert.AreEqual(111, vm.List[1]);
        }

        [TestMethod]
        public void BindingCompiler_ArrayElement_Get()
        {
            TestViewModel5 vm = new TestViewModel5();
            var result = ExecuteBinding("Array[1]", new[] { vm });
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void BindingCompiler_ArrayElement_Set()
        {
            TestViewModel5 vm = new TestViewModel5();
            ExecuteBinding("Array[1] = 111", new[] { vm }, null, expectedType: typeof(void));
            Assert.AreEqual(111, vm.Array[1]);
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
        [ExpectedExceptionMessageSubstring(typeof(BindingPropertyException), "Could not implicitly convert expression of type System.Void to System.Object")]
        public void BindingCompiler_MultiBlockExpression_EmptyBlockAtEnd_Throws()
        {
            TestViewModel vm = new TestViewModel { StringProp = "a" };
            var result = ExecuteBinding("GetEnum();", new[] { vm });
        }

        [TestMethod]
        [ExpectedExceptionMessageSubstring(typeof(BindingPropertyException), "Could not implicitly convert expression of type System.Void to System.Object")]
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
        public void BindingCompiler_MultiBlockExpression_AssignmentAtEnd_CorrectResult()
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
        public void BindingCompiler_Variables()
        {
            Assert.AreEqual(2, ExecuteBinding("var a = 1; a + 1"));
            Assert.AreEqual(typeof(int), ExecuteBinding("var a = 1; a.GetType()"));

            var result = ExecuteBinding("var a = 1; var b = a + LongProperty; var c = b + StringProp; c", new [] { new TestViewModel { LongProperty = 1, StringProp = "X" } });
            Assert.AreEqual("2X", result);
        }

        [TestMethod]
        public void BindingCompiler_VariableShadowing()
        {
            Assert.AreEqual(121L, ExecuteBinding("var LongProperty = LongProperty + 120; LongProperty", new TestViewModel { LongProperty = 1 }));
            Assert.AreEqual(7, ExecuteBinding("var a = 1; var b = (var a = 5; a + 1); a + b"));
            Assert.AreEqual(3, ExecuteBinding("var a = 1; var a = a + 1; var a = a + 1; a"));
        }

        [TestMethod]
        public void BindingCompiler_Errors_AssigningToType()
        {
            var aggEx = Assert.ThrowsException<BindingPropertyException>(() => ExecuteBinding("System.String = 123", new [] { new TestViewModel() }));
            var ex = aggEx.GetBaseException();
            StringAssert.Contains(ex.Message, "cannot be assigned into");
        }

        [TestMethod]
        public void BindingCompiler_ExclusiveOrOperator()
        {
            Assert.AreEqual(true, ExecuteBinding("var boolVariable = BoolProp ^ true; boolVariable", new TestViewModel { BoolProp = false }));
            Assert.AreEqual(false, ExecuteBinding("var boolVariable = BoolProp ^ true; boolVariable", new TestViewModel { BoolProp = true }));
        }

        [TestMethod]
        public void BindingCompiler_OnesComplementOperator()
        {
            Assert.AreEqual(-1025, ExecuteBinding("var intVariable = ~IntProp; intVariable", new TestViewModel { IntProp = 1024 }));
        }
    }
    class TestViewModel
    {
        public bool BoolProp { get; set; }
        public string StringProp { get; set; }
        public int IntProp { get; set; }
        public int? NullableIntProp { get; set; }
        public double DoubleProp { get; set; }
        public TestViewModel2 TestViewModel2 { get; set; }
        public TestViewModel2 TestViewModel2B { get; set; }
        public TestEnum EnumProperty { get; set; }
        public string StringProp2 { get; set; }
        public DateTime DateTime { get; set; }
        public DateOnly DateOnly { get; set; }
        public DateOnly? NullableDateOnly { get; set; }
        public TimeOnly TimeOnly { get; set; }
        public TimeOnly? NullableTimeOnly { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public object Time { get; set; } = TimeSpan.FromSeconds(5);
        public Guid GuidProp { get; set; }
        public Tuple<int, bool> Tuple { get; set; }
        public List<int> List { get; set; }
        public List<TestEnum> EnumList { get; set; }
        public List<string> StringList { get; set; }
        public List<long> LongList => new List<long>() { 1, 2, long.MaxValue };
        public long LongProperty { get; set; }
        public long[] LongArray => new long[] { 1, 2, long.MaxValue };
        public string[] StringArray => new string[] { "Hello ", "DotVVM" };
        public Dictionary<string, TestViewModel2> StringVmDictionary = new() { { "a", new TestViewModel2() }, { "b", new TestViewModel2() } };
        public Dictionary<int?, TestViewModel2> NullableIntVmDictionary = new() { { 0, new TestViewModel2() }, { 1, new TestViewModel2() } };
        public TestViewModel2[] VmArray => new TestViewModel2[] { new TestViewModel2() };
        public int[] IntArray { get; set; }
        public decimal DecimalProp { get; set; }

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

        public Type GetFirstGenericArgType<T, U>(Tuple<T, U> param)
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

    class TestLambdaCompilation
    {
        public string StringProp { get; set; }

        public string DelegateInvoker(Func<string, string> func) { func(default); return "Func"; }
        public string DelegateInvoker(Action<string> action) { action(default); return "Action"; }

        public string ActionInvoker(Action<string> action) { action(default); return "Action"; }
        public string Action2Invoker(Action<string, string> action) { action(default, default); return "Action"; }
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

    class TestViewModel5
    {
        public Dictionary<int, int> Dictionary { get; set; } = new Dictionary<int, int>()
        {
            { 1, 11 },
            { 2, 22 },
            { 3, 33 }
        };

        public ReadOnlyDictionary<int, int> ReadOnlyDictionary { get; set; } = new ReadOnlyDictionary<int, int>(new Dictionary<int, int>()
        {
            { 1, 11 },
            { 2, 22 },
            { 3, 33 }
        });

        public ReadOnlyCollection<int> ReadOnlyArray { get; set; } = new ReadOnlyCollection<int>(new[] { 1, 2, 3 });

        public List<int> List { get; set; } = new List<int>() { 1, 2, 3 };
        public int[] Array { get; set; } = new int[] { 1, 2, 3 };
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
        Underscore_hhh,
        [EnumMember(Value = "xxx")]
        SpecialField
    }


    public static class TestStaticClass
    {
        public static string GetSomeString() => "string 123";
    }
}
