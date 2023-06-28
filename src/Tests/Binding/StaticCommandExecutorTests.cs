using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Testing;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using static DotVVM.Framework.Testing.DotvvmTestHelper;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class StaticCommandExecutorTests
    {
        static DotvvmConfiguration config = DotvvmTestHelper.DefaultConfig;
        static readonly StaticCommandExecutor executor = config.ServiceProvider.GetRequiredService<StaticCommandExecutor>();

        StaticCommandInvocationPlan CreatePlan(Expression<Action> methodExpr)
        {
            var methodInfo = (MethodInfo)MethodFindingHelper.GetMethodFromExpression(methodExpr);
            var parameters = methodInfo.GetParameters();
            return new StaticCommandInvocationPlan(
                methodInfo,
                parameters.Select(p =>
                    p.ParameterType == typeof(ITestSingletonService) || p.ParameterType == typeof(IDotvvmRequestContext) ?
                        new StaticCommandParameterPlan(StaticCommandParameterType.Inject, p.ParameterType) :
                    new StaticCommandParameterPlan(StaticCommandParameterType.Argument, p.ParameterType)
                ).ToArray()
            );
        }

        async Task<object> Invoke(StaticCommandInvocationPlan plan, params (object value, string path)[] arguments)
        {
            var context = DotvvmTestHelper.CreateContext(config, requestType: DotvvmRequestType.StaticCommand);
            var a = arguments.Select(t => JToken.FromObject(t.value));
            var p = arguments.Select(t => t.path);
            return await executor.Execute(plan, a, p, context);
        }

        async Task<StaticCommandModelState> InvokeExpectingErrors(StaticCommandInvocationPlan plan, params (object value, string path)[] arguments)
        {
            var exception = await XAssert.ThrowsAnyAsync<Exception>(() => Invoke(plan, arguments));
            if (exception is TargetInvocationException tEx)
                exception = tEx.InnerException;
            if (exception is DotvvmInvalidStaticCommandModelStateException scEx)
                return scEx.StaticCommandModelState;
            throw new Exception("unexpected exception", exception);
        }

        [TestMethod]
        public async Task PassesRawArguments()
        {
            var plan = CreatePlan(() => ValidateRawError(null));
            var modelState = await InvokeExpectingErrors(plan, (new TestViewModel(), null));
            Assert.AreEqual(1, modelState.Errors.Count);
            Assert.IsTrue(modelState.Errors[0].IsResolved);
            Assert.AreEqual("/Some/Other/Property", modelState.Errors[0].PropertyPath);
        }

        [AllowStaticCommand]
        internal static void ValidateRawError(TestViewModel vm)
        {
            var ms = new StaticCommandModelState();
            ms.AddRawError("/Some/Other/Property", "error");
            ms.FailOnInvalidModelState();
        }

        [TestMethod]
        public async Task PlainArgumentError()
        {
            var plan = CreatePlan(() => ValidatePlainArguments(null, null, null, null, null));
            var modelState = await InvokeExpectingErrors(plan, ("value", "/Property1"), ("value", "/Property2"));
            Assert.AreEqual(2, modelState.Errors.Count);
            Assert.IsTrue(modelState.Errors[0].IsResolved);
            Assert.AreEqual("/Property1", modelState.Errors[0].PropertyPath);
            Assert.IsTrue(modelState.Errors[1].IsResolved);
            Assert.AreEqual("/Property2", modelState.Errors[1].PropertyPath);
        }
        [AllowStaticCommand]
        internal static Task ValidatePlainArguments(ITestSingletonService service, IDotvvmRequestContext context1, string a, IDotvvmRequestContext context2, string b)
        {
            var ms = new StaticCommandModelState();
            ms.AddArgumentError(nameof(a), "error1");
            ms.AddArgumentError(() => b, "error2");
            ms.FailOnInvalidModelState();
            return Task.CompletedTask;
        }

        [TestMethod]
        public async Task ArgumentPropertyError()
        {
#pragma warning disable CS4014 // awaitable not awaited warning
            var plan = CreatePlan(() => ValidateArgumentProperty(null, null));
#pragma warning restore CS4014
            var modelState = await InvokeExpectingErrors(plan, (new TestViewModel(), "/MyViewModel"));
            Assert.AreEqual(2, modelState.Errors.Count);
            Assert.IsTrue(modelState.Errors[0].IsResolved);
            Assert.AreEqual("/MyViewModel/IntProp", modelState.Errors[0].PropertyPath);
            Assert.IsTrue(modelState.Errors[1].IsResolved);
            Assert.AreEqual("/MyViewModel/DoubleProp", modelState.Errors[1].PropertyPath);
        }
        [AllowStaticCommand]
        internal static async Task<bool> ValidateArgumentProperty(IDotvvmRequestContext context, TestViewModel vm)
        {
            var ms = new StaticCommandModelState();
            ms.AddRawArgumentError(nameof(vm), "/IntProp", "error1");
            ms.AddRawArgumentError("vm", "DoubleProp", "error2");
            ms.FailOnInvalidModelState();
            return true;
        }

        [TestMethod]
        public async Task ThrowsWhenValueTaskIsUsed()
        {
#pragma warning disable CS4014 // awaitable not awaited warning
            var plan = CreatePlan(() => ReturningValueTask());
#pragma warning restore CS4014
            var exception = await XAssert.ThrowsAnyAsync<Exception>(() => Invoke(plan));
            if (exception is TargetInvocationException tEx)
                exception = tEx.InnerException;
            Assert.IsInstanceOfType(exception, typeof(NotSupportedException));
            XAssert.Contains("The command uses unsupported awaitable type", exception.Message);
        }

        [AllowStaticCommand]
        internal static async ValueTask<bool> ReturningValueTask()
        {
            return true;
        }
    }
}
