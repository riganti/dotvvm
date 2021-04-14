using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Runtime.InteropServices.ComTypes;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Binding
{
    [TestClass]
    public class ExpressionHelperTests
    {
        public TestContext TestContext { get; set; }
        private MemberExpressionFactory memberExpressionFactory;

        [TestInitialize]
        public void Init()
        {
            var configuration = DotvvmTestHelper.CreateConfiguration();
            var extensionsCache = configuration.ServiceProvider.GetRequiredService<ExtensionMethodsCache>();
            memberExpressionFactory = new MemberExpressionFactory(extensionsCache);
        }

        [TestMethod]
        public void UpdateMember_GetValue()
        {
            var cP = Expression.Parameter(typeof(DotvvmControl), "c");
            var newValueP = Expression.Parameter(typeof(object), "newValue");
            var updateExpr = memberExpressionFactory.UpdateMember(ExpressionUtils.Replace((DotvvmControl c) => c.GetValue(DotvvmBindableObject.DataContextProperty, true), cP), newValueP);
            Assert.IsNotNull(updateExpr);
            Assert.AreEqual("c.SetValue(DotvvmBindableObject.DataContextProperty, newValue)", updateExpr.ToString());
        }

        [TestMethod]
        public void UpdateMember_NormalProperty()
        {
            var vmP = Expression.Parameter(typeof(Tests.Binding.TestViewModel), "vm");
            var newValueP = Expression.Parameter(typeof(DateTime), "newValue");
            var updateExpr = memberExpressionFactory.UpdateMember(ExpressionUtils.Replace((Tests.Binding.TestViewModel c) => c.DateFrom, vmP), newValueP);
            Assert.IsNotNull(updateExpr);
            Assert.AreEqual("(vm.DateFrom = Convert(newValue, Nullable`1))", updateExpr.ToString());
        }

        [TestMethod]
        public void UpdateMember_ReadOnlyProperty()
        {
            var vmP = Expression.Parameter(typeof(Tests.Binding.TestViewModel), "vm");
            var newValueP = Expression.Parameter(typeof(long[]), "newValue");
            var updateExpr = memberExpressionFactory.UpdateMember(ExpressionUtils.Replace((Tests.Binding.TestViewModel c) => c.LongArray, vmP), newValueP);
            Assert.IsNull(updateExpr);
        }

        [TestMethod]
        [DataRow(typeof(GenericTestResult1), typeof(int), new Type[0])]
        [DataRow(typeof(GenericTestResult2), typeof(Uri), new Type[] { typeof(Uri) })]
        [DataRow(typeof(GenericTestResult3), typeof(object), new Type[0])]
        [DataRow(typeof(GenericTestResult5), typeof(GenericModelSampleObject<int>), new Type[0])]
        public void Call_FindOverload_Generic_FirstLevel(Type resultIdentifierType, Type argType, Type[] expectedGenericArgs)
        {
            Call_FindOverload_Generic(typeof(MethodsGenericArgumentsResolvingSampleObject), MethodsGenericArgumentsResolvingSampleObject.MethodName, new[] { argType }, resultIdentifierType, expectedGenericArgs);
        }

        private void Call_FindOverload_Generic(Type targetType, string methodName, Type[] argTypes, Type resultIdentifierType, Type[] expectedGenericArgs)
        {
            Expression target = new MethodGroupExpression() {
                MethodName = methodName,
                Target = new StaticClassIdentifierExpression(targetType)
            };

            var j = 0;
            var arguments = argTypes.Select(s => Expression.Parameter(s, $"param_{j++}")).ToArray();
            var expression = memberExpressionFactory.Call(target, arguments) as MethodCallExpression;
            Assert.IsNotNull(expression);
            Assert.AreEqual(resultIdentifierType, expression.Method.GetResultType());

            var args = expression.Method.GetGenericArguments();
            for (int i = 0; i < args.Length; i++)
            {
                Assert.AreEqual(expectedGenericArgs[i], args[i], message: "Order of resolved generic types is different then expected.");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [DataRow(typeof(GenericModelSampleObject<string>), typeof(GenericTestResult4), new Type[] { typeof(string) })]
        public void Call_FindOverload_Generic_Ambiguous(Type argType, Type resultIdentifierType, Type[] expectedGenericArgs)
        {
            Call_FindOverload_Generic(typeof(MethodsGenericArgumentsResolvingSampleObject), MethodsGenericArgumentsResolvingSampleObject.MethodName, new[] { argType }, resultIdentifierType, expectedGenericArgs);
        }


        [TestMethod]
        [DataRow(typeof(GenericTestResult2), new Type[] { typeof(GenericModelSampleObject<GenericModelSampleObject<string>>), typeof(int) }, new Type[] { typeof(int), typeof(string) })]
        public void Call_FindOverload_Generic_Ambiguous_Recursive(Type resultIdentifierType, Type[] argTypes, Type[] expectedGenericArgs)
        {
            Call_FindOverload_Generic(typeof(MethodsGenericArgumentsResolvingSampleObject2), MethodsGenericArgumentsResolvingSampleObject2.MethodName, argTypes, resultIdentifierType, expectedGenericArgs);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [DataRow(typeof(GenericTestResult1), new Type[] { typeof(GenericModelSampleObject<GenericModelSampleObject<string>>), typeof(GenericModelSampleObject<GenericModelSampleObject<int>>) }, new Type[] { typeof(string), typeof(int) })]
        public void Call_FindOverload_Generic_Ambiguous_Recursive_CannotDetermineResult(Type resultIdentifierType, Type[] argTypes, Type[] expectedGenericArgs)
        {
            // This call should probably return method with result GenericTestResult1, but there is no certainty that it is what c# would resulted.
            Call_FindOverload_Generic(typeof(MethodsGenericArgumentsResolvingSampleObject2), MethodsGenericArgumentsResolvingSampleObject2.MethodName, argTypes, resultIdentifierType, expectedGenericArgs);
        }

        [TestMethod]
        [DataRow(typeof(GenericTestResult1), new Type[] { typeof(int), typeof(string) }, new Type[] { typeof(string), typeof(int) })]
        public void Call_FindOverload_Generic_Order_FirstLevel(Type resultIdentifierType, Type[] argTypes, Type[] expectedGenericArgs)
        {
            Call_FindOverload_Generic(typeof(MethodsGenericArgumentsResolvingSampleObject3), MethodsGenericArgumentsResolvingSampleObject3.MethodName, argTypes, resultIdentifierType, expectedGenericArgs);
        }

        [TestMethod]
        [DataRow(typeof(GenericTestResult1), new Type[] { typeof(int[]), typeof(string[]) }, new Type[] { typeof(string[]), typeof(int[]) })]
        public void Call_FindOverload_Generic_Array_Order(Type resultIdentifierType, Type[] argTypes, Type[] expectedGenericArgs)
        {
            Call_FindOverload_Generic(typeof(MethodsGenericArgumentsResolvingSampleObject3), MethodsGenericArgumentsResolvingSampleObject3.MethodName, argTypes, resultIdentifierType, expectedGenericArgs);
        }
        [TestMethod]
        [DataRow(typeof(int[]), new Type[] { typeof(int[]) }, new Type[] { typeof(int) })]
        public void Call_FindOverload_Generic_Array(Type resultIdentifierType, Type[] argTypes, Type[] expectedGenericArgs)
        {
            Call_FindOverload_Generic(typeof(MethodsGenericArgumentsResolvingSampleObject4), MethodsGenericArgumentsResolvingSampleObject4.MethodName, argTypes, resultIdentifierType, expectedGenericArgs);
        }
        [TestMethod]
        [DataRow(typeof(int), new Type[] { typeof(int[]) }, new Type[] { typeof(int) })]
        public void Call_FindOverload_Generic_Enumerable_Array(Type resultIdentifierType, Type[] argTypes, Type[] expectedGenericArgs)
        {
            Call_FindOverload_Generic(typeof(MethodsGenericArgumentsResolvingSampleObject6), MethodsGenericArgumentsResolvingSampleObject6.MethodName, argTypes, resultIdentifierType, expectedGenericArgs);
        }
        [TestMethod]
        [DataRow(typeof(GenericTestResult1), new Type[] { typeof(List<int>), typeof(GenericInterfaceIntImplementation) }, new Type[] { typeof(int) })]
        [DataRow(typeof(GenericTestResult1), new Type[] { typeof(List<int>), typeof(GenericInterfaceFloatImplementation) }, new Type[] { typeof(float) })]
        [DataRow(typeof(GenericTestResult2), new Type[] { typeof(List<int>), typeof(DerivedFromGenericClassInt) }, new Type[] { typeof(int) })]
        [DataRow(typeof(GenericTestResult3), new Type[] { typeof(List<int>), typeof(MultiGenericInterfaceImplementation) }, new Type[] { typeof(int), typeof(string) })]
        [DataRow(typeof(GenericTestResult3), new Type[] { typeof(List<int>), typeof(GenericInterfaceStringImplementation) }, new Type[] { typeof(string) })]
        [DataRow(typeof(GenericTestResult4), new Type[] { typeof(List<int>), typeof(DerivedFromGenericClassString) }, new Type[] { typeof(string) })]
        [DataRow(typeof(GenericTestResult5), new Type[] { typeof(List<int>), typeof(GenericInterfaceIntImplementation), typeof(GenericInterfaceStringImplementation) }, new Type[] { typeof(int), typeof(string) })]
        [DataRow(typeof(GenericTestResult6), new Type[] { typeof(List<int>), typeof(GenericInterfaceIntImplementation), typeof(GenericInterfaceFloatImplementation) }, new Type[] { typeof(int) })]
        [DataRow(typeof(GenericTestResult7), new Type[] { typeof(List<GenericInterfaceIntImplementation>) }, new Type[] { typeof(int) })]
        [DataRow(typeof(GenericTestResult7), new Type[] { typeof(HashSet<GenericInterfaceIntImplementation>) }, new Type[] { typeof(int) })]
        [DataRow(typeof(GenericTestResult8), new Type[] { typeof(List<GenericInterfaceStringImplementation>) }, new Type[] { typeof(string) })]
        public void Call_FindOverload_Generic_ImplicitConversions(Type resultIdentifierType, Type[] argTypes, Type[] expectedGenericArgs)
        {
            Call_FindOverload_Generic(typeof(ImplicitConversionsTest), nameof(ImplicitConversionsTest.Method), argTypes, resultIdentifierType, expectedGenericArgs);
        }
        [TestMethod]
        [DataRow(typeof(GenericTestResult1), new Type[] { typeof(GenericModelSampleObject<int[]>) }, new Type[] { typeof(int) })]
        [DataRow(typeof(GenericTestResult2), new Type[] { typeof(List<int>[]) }, new Type[] { typeof(int) })]
        public void Call_FindOverload_Generic_Array_Recursive(Type resultIdentifierType, Type[] argTypes, Type[] expectedGenericArgs)
        {
            Call_FindOverload_Generic(typeof(MethodsGenericArgumentsResolvingSampleObject5), MethodsGenericArgumentsResolvingSampleObject5.MethodName, argTypes, resultIdentifierType, expectedGenericArgs);
        }

        [TestMethod]
        [DataRow(typeof(GenericTestResult1), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }, new Type[] { typeof(int[]) })]
        [DataRow(typeof(GenericTestResult2), new Type[] { typeof(string), typeof(int), typeof(int), typeof(int) }, new Type[] { typeof(string), typeof(object[]) })]
        [DataRow(typeof(GenericTestResult2), new Type[] { typeof(string), typeof(int), typeof(string), typeof(int) }, new Type[] { typeof(string), typeof(object[]) })]
        public void Call_FindOverload_Params_Array(Type resultIdentifierType, Type[] argTypes, Type[] expectedArgsTypes)
        {
            Expression target = new MethodGroupExpression() {
                MethodName = MethodsParamsArgumentsResolvingSampleObject.MethodName,
                Target = new StaticClassIdentifierExpression(typeof(MethodsParamsArgumentsResolvingSampleObject))
            };

            var j = 0;
            var arguments = argTypes.Select(s => Expression.Parameter(s, $"param_{j++}")).ToArray();
            var expression = memberExpressionFactory.Call(target, arguments) as MethodCallExpression;
            Assert.IsNotNull(expression);
            Assert.AreEqual(resultIdentifierType, expression.Method.GetResultType());

            var args = expression.Arguments.Select(s => s.Type).ToArray();
            for (int i = 0; i < args.Length; i++)
            {
                Assert.AreEqual(expectedArgsTypes[i], args[i], message: "Order of resolved generic types is different then expected.");
            }
        }

        [TestMethod]
        [DataRow(typeof(GenericTestResult1), new Type[] { /* params empty */ }, new Type[] { typeof(int[]) })]
        [DataRow(typeof(GenericTestResult2), new Type[] { typeof(string), /* params empty */ }, new Type[] { typeof(string), typeof(object[]) })]
        [DataRow(typeof(GenericTestResult3), new Type[] { typeof(bool), /* params empty */ }, new Type[] { typeof(bool), typeof(int[]) })]
        [DataRow(typeof(GenericTestResult4), new Type[] { typeof(float), typeof(double), /* params empty */ }, new Type[] { typeof(float), typeof(double), typeof(int[]) })]
        [DataRow(typeof(GenericTestResult4), new Type[] { typeof(float), /* default argument, params empty */ }, new Type[] { typeof(float), typeof(double), typeof(int[]) })]
        public void Call_FindOverload_Params_Empty(Type resultIdentifierType, Type[] argTypes, Type[] expectedArgsTypes)
        {
            Expression target = new MethodGroupExpression() {
                MethodName = MethodsParamsArgumentsResolvingSampleObject.MethodName,
                Target = new StaticClassIdentifierExpression(typeof(MethodsParamsArgumentsResolvingSampleObject))
            };

            var j = 0;
            var arguments = argTypes.Select(s => Expression.Parameter(s, $"param_{j++}")).ToArray();
            var expression = memberExpressionFactory.Call(target, arguments) as MethodCallExpression;
            Assert.IsNotNull(expression);
            Assert.AreEqual(resultIdentifierType, expression.Method.GetResultType());

            var args = expression.Arguments.Select(s => s.Type).ToArray();
            for (var i = 0; i < args.Length; i++)
            {
                Assert.AreEqual(expectedArgsTypes[i], args[i], message: "Order of resolved generic types is different then expected.");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [DataRow(typeof(GenericTestResult1), new Type[] { typeof(int), typeof(int), typeof(string), typeof(int) }, new Type[] { typeof(int[]) })]
        [DataRow(typeof(GenericTestResult3), new Type[] { typeof(bool), typeof(int), typeof(string), typeof(int) }, new Type[] { typeof(bool), typeof(int[]) })]
        public void Call_FindOverload_Params_Array_Invalid(Type resultIdentifierType, Type[] argTypes, Type[] expectedArgsTypes)
        {
            Expression target = new MethodGroupExpression() {
                MethodName = MethodsParamsArgumentsResolvingSampleObject.MethodName,
                Target = new StaticClassIdentifierExpression(typeof(MethodsParamsArgumentsResolvingSampleObject))
            };

            var j = 0;
            var arguments = argTypes.Select(s => Expression.Parameter(s, $"param_{j++}")).ToArray();
            var expression = memberExpressionFactory.Call(target, arguments) as MethodCallExpression;
            Assert.IsNotNull(expression);
            Assert.AreEqual(resultIdentifierType, expression.Method.GetResultType());

            var args = expression.Arguments.Select(s => s.Type).ToArray();
            for (int i = 0; i < args.Length; i++)
            {
                Assert.AreEqual(expectedArgsTypes[i], args[i], message: "Order of resolved generic types is different then expected.");
            }
        }

        [TestMethod]
        [DataRow(new Type[] { typeof(string) }, typeof(string))]
        [DataRow(new Type[] { typeof(string), typeof(object) }, typeof((string, object)))]
        [DataRow(new Type[] { typeof(string), typeof(object), typeof(object) }, typeof((string, object[])))]
        [DataRow(new Type[] { typeof(string), typeof(string) }, typeof((string, string[])))]
        [DataRow(new Type[] { typeof(string), typeof(int) }, typeof((string, int[])))]
        public void Call_FindOverload_DoNotPrioritizeParams(Type[] argTypes, Type resultType)
        {
            Expression target = new MethodGroupExpression() {
                MethodName = nameof(ParamsPrioritizationTest.Method),
                Target = new StaticClassIdentifierExpression(typeof(ParamsPrioritizationTest))
            };

            var j = 0;
            var arguments = argTypes.Select(s => Expression.Parameter(s, $"param_{j++}")).ToArray();
            var expression = memberExpressionFactory.Call(target, arguments) as MethodCallExpression;
            Assert.IsNotNull(expression);
            Assert.AreEqual(resultType, expression.Method.GetResultType());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [DataRow(typeof(GenericTestResult1), new Type[] { typeof(string), typeof(string), typeof(string) }, new Type[] { typeof(string) })]
        public void Call_FindOverload_Params_Generic_Array(Type resultIdentifierType, Type[] argTypes, Type[] expectedArgsTypes)
        {
            Call_FindOverload_Generic(typeof(MethodsParamsArgumentsGenericResolvingSampleObject),
                MethodsParamsArgumentsGenericResolvingSampleObject.MethodName, argTypes, resultIdentifierType, expectedArgsTypes);
        }

        [TestMethod]
        [DataRow(typeof(GenericTestResult1), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }, new Type[] { typeof(int) })]
        [DataRow(typeof(GenericTestResult2), new Type[] { typeof(string), typeof(int), typeof(int), typeof(int) }, new Type[] { typeof(int) })]
        [DataRow(typeof(GenericTestResult2), new Type[] { typeof(double), typeof(bool), typeof(bool) }, new Type[] { typeof(double) })]
        public void Call_FindOverload_Params_Generic_Array_Invalid(Type resultIdentifierType, Type[] argTypes, Type[] expectedArgsTypes)
        {
            Call_FindOverload_Generic(typeof(MethodsParamsArgumentsGenericResolvingSampleObject),
                MethodsParamsArgumentsGenericResolvingSampleObject.MethodName, argTypes, resultIdentifierType, expectedArgsTypes);
        }
    }
    public static class MethodsParamsArgumentsGenericResolvingSampleObject
    {
        public const string MethodName = nameof(TestMethod);
        public static GenericTestResult1 TestMethod<T>(params T[] data) => null;
        public static GenericTestResult2 TestMethod<T>(string value, params T[] data) => null;
        public static GenericTestResult2 TestMethod<T>(T value, params bool[] data) => null;
    }
    public static class MethodsParamsArgumentsResolvingSampleObject
    {
        public const string MethodName = nameof(TestMethod);
        public static GenericTestResult1 TestMethod(params int[] data) => null;
        public static GenericTestResult2 TestMethod(string value, params object[] data) => null;
        public static GenericTestResult3 TestMethod(bool value, params int[] data) => null;
        public static GenericTestResult4 TestMethod(float value, double defaultValue = 3.5, params int[] data) => null;
    }

    public static class MethodsGenericArgumentsResolvingSampleObject6
    {
        public const string MethodName = nameof(TestMethod);
        public static T2 TestMethod<T2>(IEnumerable<T2> a) => default;
    }
    public static class MethodsGenericArgumentsResolvingSampleObject5
    {
        public const string MethodName = nameof(TestMethod);
        public static GenericTestResult1 TestMethod<T1>(GenericModelSampleObject<T1[]> a) => null;
        public static GenericTestResult2 TestMethod<T1>(List<T1>[] a) => null;
    }
    public static class MethodsGenericArgumentsResolvingSampleObject4
    {
        public const string MethodName = nameof(TestMethod);
        public static T1[] TestMethod<T1>(T1[] a) => null;
    }
    public static class MethodsGenericArgumentsResolvingSampleObject3
    {
        public const string MethodName = nameof(TestMethod);
        public static GenericTestResult1 TestMethod<T1, T2>(T2 a, T1 b) => null;
    }
    public static class MethodsGenericArgumentsResolvingSampleObject2
    {
        public const string MethodName = nameof(TestMethod);
        public static GenericTestResult1 TestMethod<T1, T2>(GenericModelSampleObject<GenericModelSampleObject<T1>> a, GenericModelSampleObject<GenericModelSampleObject<T2>> b) => null;
        public static GenericTestResult2 TestMethod<T2, T1>(GenericModelSampleObject<GenericModelSampleObject<T1>> a, T2 b) => null;
    }
    public static class MethodsGenericArgumentsResolvingSampleObject
    {
        public const string MethodName = nameof(TestMethod);
        public static GenericTestResult1 TestMethod(int a) => null;
        public static GenericTestResult2 TestMethod<T>(T a) => null;
        public static GenericTestResult3 TestMethod(object a) => null;
        public static GenericTestResult4 TestMethod<T>(GenericModelSampleObject<T> a) => null;
        public static GenericTestResult5 TestMethod(GenericModelSampleObject<int> a) => null;
    }

    public class GenericModelSampleObject<T>
    {
        public T Prop { get; set; }
    }

    public class GenericTestResult1 { }
    public class GenericTestResult2 { }
    public class GenericTestResult3 { }
    public class GenericTestResult4 { }
    public class GenericTestResult5 { }
    public class GenericTestResult6 { }
    public class GenericTestResult7 { }
    public class GenericTestResult8 { }

    public class ParamsPrioritizationTest
    {
        public static string Method(string arg1) => default;
        public static (string, object[]) Method(string arg1, params object[] arg2) => default;
        public static (string, string[]) Method(string arg1, params string[] arg2) => default;
        public static (string, int[]) Method(string arg1, params int[] arg2) => default;
        public static (string, object) Method(string arg1, object arg2) => default;
    }

    public interface GenericInterface<T> { }
    public class GenericClass<T> { }
    public class GenericInterfaceIntImplementation : GenericInterface<int> { }
    public class GenericInterfaceFloatImplementation : GenericInterface<float> { }
    public class GenericInterfaceStringImplementation : GenericInterface<string> { }
    public class MultiGenericInterfaceImplementation : GenericInterface<float>, GenericInterface<string> { }
    public class DerivedFromGenericClassInt : GenericClass<int> { }
    public class DerivedFromGenericClassString : GenericClass<string> { }

    public class ImplicitConversionsTest
    {
        public static GenericTestResult1 Method<T>(List<int> arg1, GenericInterface<T> arg2) => default;
        public static GenericTestResult2 Method<T>(List<int> arg1, GenericClass<T> arg2) => default;

        public static GenericTestResult3 Method(List<int> arg1, GenericInterface<string> arg2) => default;
        public static GenericTestResult4 Method(List<int> arg1, GenericClass<string> arg2) => default;

        public static GenericTestResult5 Method<T, U>(List<int> arg1, GenericInterface<T> arg2, GenericInterface<U> arg3) => default;
        public static GenericTestResult6 Method<T>(List<int> arg1, GenericInterface<T> arg2, GenericInterface<float> arg3) => default;

        public static GenericTestResult7 Method<T>(IEnumerable<GenericInterface<T>> arg1) => default;
        public static GenericTestResult8 Method(IEnumerable<GenericInterface<string>> arg1) => default;
    }
}
