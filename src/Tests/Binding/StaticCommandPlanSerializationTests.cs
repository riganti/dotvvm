using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class StaticCommandPlanSerializationTests
    {
        private StaticCommandInvocationPlan MakeInvocationPlan(Expression<Action> methodCapture, params StaticCommandParameterPlan[] parameters)
        {
            var method = ((MethodCallExpression)methodCapture.Body).Method;
            return new StaticCommandInvocationPlan(method, parameters);
        }

        private void AssertPlansAreIdentical(StaticCommandInvocationPlan original, StaticCommandInvocationPlan deserialized)
        {
            Assert.AreEqual(original.Method, deserialized.Method);
            Assert.AreEqual(original.Arguments.Length, deserialized.Arguments.Length);

            var index = 0;
            foreach (var arg in original.Arguments)
            {
                Assert.AreEqual(arg.Type, deserialized.Arguments[index].Type);
                Assert.AreEqual(arg.Arg, deserialized.Arguments[index].Arg);
                index++;
            }
        }

        [TestMethod]
        public void StaticCommandPlanSerialization_MethodOverloads1_DeserializedPlanIsIdentical()
        {
            var plan = MakeInvocationPlan(() => StaticCommandMethodCollection.Method(123),
                new StaticCommandParameterPlan(StaticCommandParameterType.Constant, 123));
            var json = StaticCommandExecutionPlanSerializer.SerializePlan(plan);
            var deserializedPlan = StaticCommandExecutionPlanSerializer.DeserializePlan(json);

            AssertPlansAreIdentical(plan, deserializedPlan);
        }

        [TestMethod]
        public void StaticCommandPlanSerialization_MethodOverloads2_DeserializedPlanIsIdentical()
        {
            var plan = MakeInvocationPlan(() => StaticCommandMethodCollection.Method(123f),
                new StaticCommandParameterPlan(StaticCommandParameterType.Constant, 123f));
            var json = StaticCommandExecutionPlanSerializer.SerializePlan(plan);
            var deserializedPlan = StaticCommandExecutionPlanSerializer.DeserializePlan(json);

            AssertPlansAreIdentical(plan, deserializedPlan);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void StaticCommandPlanSerialization_NotUsableInStaticCommand_Throws()
        {
            var plan = MakeInvocationPlan(() => StaticCommandMethodCollection.MethodNotUsableInStaticCommand());
            var json = StaticCommandExecutionPlanSerializer.SerializePlan(plan);
            var deserializedPlan = StaticCommandExecutionPlanSerializer.DeserializePlan(json);
        }

        static class StaticCommandMethodCollection
        {
            [AllowStaticCommand]
            public static void Method(int arg) { }

            [AllowStaticCommand]
            public static void Method(float arg) { }


            public static void MethodNotUsableInStaticCommand() { }
        }
    }
}
