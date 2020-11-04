using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Runtime.InteropServices.ComTypes;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Binding
{
    [TestClass]
    public class ExpressionHelperTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void UpdateMember_GetValue()
        {
            var cP = Expression.Parameter(typeof(DotvvmControl), "c");
            var newValueP = Expression.Parameter(typeof(object), "newValue");
            var updateExpr = ExpressionHelper.UpdateMember(ExpressionUtils.Replace((DotvvmControl c) => c.GetValue(DotvvmBindableObject.DataContextProperty, true), cP), newValueP);
            Assert.IsNotNull(updateExpr);
            Assert.AreEqual("c.SetValue(DotvvmBindableObject.DataContextProperty, newValue)", updateExpr.ToString());
        }

        [TestMethod]
        public void UpdateMember_NormalProperty()
        {
            var vmP = Expression.Parameter(typeof(Tests.Binding.TestViewModel), "vm");
            var newValueP = Expression.Parameter(typeof(DateTime), "newValue");
            var updateExpr = ExpressionHelper.UpdateMember(ExpressionUtils.Replace((Tests.Binding.TestViewModel c) => c.DateFrom, vmP), newValueP);
            Assert.IsNotNull(updateExpr);
            Assert.AreEqual("(vm.DateFrom = Convert(newValue, Nullable`1))", updateExpr.ToString());
        }

        [TestMethod]
        public void UpdateMember_ReadOnlyProperty()
        {
            var vmP = Expression.Parameter(typeof(Tests.Binding.TestViewModel), "vm");
            var newValueP = Expression.Parameter(typeof(long[]), "newValue");
            var updateExpr = ExpressionHelper.UpdateMember(ExpressionUtils.Replace((Tests.Binding.TestViewModel c) => c.LongArray, vmP), newValueP);
            Assert.IsNull(updateExpr);
        }

        [TestMethod]
        [DataRow(typeof(GenericTestResult1), typeof(int), new Type[0])]
        //[DataRow(typeof(GenericTestResult2), typeof(Uri), new Type[] { typeof(Uri) })]
        //[DataRow(typeof(GenericTestResult3), typeof(object), new Type[0])]
        //[DataRow(typeof(GenericTestResult5), typeof(GenericModelSampleObject<int>), new Type[0])]
        public void Call_FindOverload_Generic_FirstLevel(Type resultIdentifierType, Type argType, Type[] expectedGenericArgs)
        {
            Call_FindOverload_Generic(typeof(MethodsGenericArgumentsResolvingSampleObject), MethodsGenericArgumentsResolvingSampleObject.MethodName, new[] { argType }, resultIdentifierType, expectedGenericArgs);
        }

        private static void Call_FindOverload_Generic(Type targetType, string methodName, Type[] argTypes, Type resultIdentifierType, Type[] expectedGenericArgs)
        {
            Expression target = new MethodGroupExpression() {
                MethodName = methodName,
                Target = new StaticClassIdentifierExpression(targetType)
            };

            var j = 0;
            var arguments = argTypes.Select(s => Expression.Parameter(s, $"param_{j++}")).ToArray();
            var expression = ExpressionHelper.Call(target, arguments) as MethodCallExpression;
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

}
