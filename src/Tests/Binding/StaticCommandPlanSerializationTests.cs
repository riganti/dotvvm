﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        private StaticCommandInvocationPlan Deserialize(JsonNode json)
        {
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json.ToJsonString()));
            return StaticCommandExecutionPlanSerializer.DeserializePlan(ref reader);
        }

        [TestMethod]
        public void StaticCommandPlanSerialization_MethodOverloads1_DeserializedPlanIsIdentical()
        {
            var plan = MakeInvocationPlan(() => StaticCommandMethodCollection.Method(123),
                new StaticCommandParameterPlan(StaticCommandParameterType.Constant, 123));
            var json = StaticCommandExecutionPlanSerializer.SerializePlan(plan);
            var deserializedPlan = Deserialize(json);

            AssertPlansAreIdentical(plan, deserializedPlan);
        }

        [TestMethod]
        public void StaticCommandPlanSerialization_MethodOverloads2_DeserializedPlanIsIdentical()
        {
            var plan = MakeInvocationPlan(() => StaticCommandMethodCollection.Method(123f),
                new StaticCommandParameterPlan(StaticCommandParameterType.Constant, 123f));
            var json = StaticCommandExecutionPlanSerializer.SerializePlan(plan);
            var deserializedPlan = Deserialize(json);

            AssertPlansAreIdentical(plan, deserializedPlan);
        }

        [TestMethod]
        public void StaticCommandPlanSerialization_NotOverloadedMethod_DoNotTransferParameterTypeNames()
        {
            var plan = MakeInvocationPlan(() => StaticCommandMethodCollection.NotOverloadedMethod(123),
                new StaticCommandParameterPlan(StaticCommandParameterType.Constant, 123));
            var json = StaticCommandExecutionPlanSerializer.SerializePlan(plan);

            var jarray = (JsonArray)json;
            // Parameters count
            Assert.AreEqual(1, (int)jarray[3]);
            // No parameters info is sent because method name and arguments are enough to match correct method
            Assert.IsNull(jarray[4]);
        }

        [TestMethod]
        public void StaticCommandPlanSerialization_OverloadedMethod_TransferParameterTypeNames()
        {
            var plan = MakeInvocationPlan(() => StaticCommandMethodCollection.Method(123),
                new StaticCommandParameterPlan(StaticCommandParameterType.Constant, 123));
            var json = StaticCommandExecutionPlanSerializer.SerializePlan(plan);

            var jarray = (JsonArray)json;
            // Parameters count
            Assert.AreEqual(1, (int)jarray[3]);
            // Parameters info is sent because method has multiple overloads
            Assert.IsNotNull(jarray[4]);
            Assert.IsInstanceOfType(jarray[4], typeof(JsonArray));
            var parameterTypeName = (string)jarray[4][0];
            Assert.AreEqual(typeof(int), Type.GetType(parameterTypeName));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void StaticCommandPlanSerialization_NotUsableInStaticCommand_Throws()
        {
            var plan = MakeInvocationPlan(() => StaticCommandMethodCollection.MethodNotUsableInStaticCommand());
            var json = StaticCommandExecutionPlanSerializer.SerializePlan(plan);
            var deserializedPlan = Deserialize(json);
        }

        static class StaticCommandMethodCollection
        {
            [AllowStaticCommand]
            public static void Method(int arg) { }

            [AllowStaticCommand]
            public static void Method(float arg) { }

            [AllowStaticCommand]
            public static void NotOverloadedMethod(int arg) { }

            public static void MethodNotUsableInStaticCommand() { }
        }
    }
}
