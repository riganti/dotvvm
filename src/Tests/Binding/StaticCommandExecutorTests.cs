using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            var parameters = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            if (!methodInfo.IsStatic)   
                parameters = Enumerable.Concat(new Type[] { methodInfo.DeclaringType }, parameters).ToArray();
            return new StaticCommandInvocationPlan(
                methodInfo,
                parameters.Select(p =>
                    p == typeof(ITestSingletonService) || p == typeof(IDotvvmRequestContext) ?
                        new StaticCommandParameterPlan(StaticCommandParameterType.Inject, p) :
                    new StaticCommandParameterPlan(StaticCommandParameterType.Argument, p)
                ).ToArray()
            );
        }

        async Task<object> Invoke(StaticCommandInvocationPlan plan, params (object value, string path)[] arguments)
        {
            var context = DotvvmTestHelper.CreateContext(config, requestType: DotvvmRequestType.StaticCommand);
            var a = arguments.Select(t => JToken.FromObject(t.value, DefaultSerializerSettingsProvider.CreateJsonSerializer()));
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
        public async Task Validation_PassesRawArguments()
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
        public async Task Validation_PlainArgumentError()
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
        public async Task Validation_ArgumentPropertyError()
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
        public async Task Validation_ThrowsWhenValueTaskIsUsed()
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
        [TestMethod]
        public async Task Validation_ArgumentPropertyLambdaError()
        {
#pragma warning disable CS4014 // awaitable not awaited warning
            var plan = CreatePlan(() => ValidateArgumentPropertyLambda(null, null));
#pragma warning restore CS4014
            var modelState = await InvokeExpectingErrors(plan, (new TestViewModel(), "/MyViewModel"));
            Assert.AreEqual(3, modelState.Errors.Count);
            Assert.IsTrue(modelState.Errors[0].IsResolved);
            Assert.AreEqual("/MyViewModel/IntProp", modelState.Errors[0].PropertyPath);
            Assert.IsTrue(modelState.Errors[1].IsResolved);
            Assert.AreEqual("/MyViewModel/VmArray/12/Enum", modelState.Errors[1].PropertyPath);
            Assert.IsTrue(modelState.Errors[2].IsResolved);
            Assert.AreEqual("/MyViewModel/LongList/12", modelState.Errors[2].PropertyPath);
        }
        [AllowStaticCommand]
        internal static async Task<bool> ValidateArgumentPropertyLambda(IDotvvmRequestContext context, TestViewModel vm)
        {
            var ms = new StaticCommandModelState();
            ms.AddArgumentError(() => vm.IntProp, "error1");
            var index = 12;
            ms.AddArgumentError(() => vm.VmArray[index].Enum, "error2");
            ms.AddArgumentError(() => vm.LongList[index], "error3");
            ms.FailOnInvalidModelState();
            return true;
        }

        [TestMethod]
        public async Task Validation_InstanceCall()
        {
            var vm = new ViewModelInstance { Property = "abab" };
            var plan = CreatePlan(() => vm.ValidatedStaticCommand(null));
            var modelState = await InvokeExpectingErrors(plan, (vm, "/VM_this"), (new TestViewModel(), "/VM_argument"));
            Assert.AreEqual(1, modelState.Errors.Count, $"Unexpected errors: {string.Join(", ", modelState.Errors)}");
            Assert.IsTrue(modelState.Errors[0].IsResolved);
            Assert.AreEqual("manual-error", modelState.Errors[0].ErrorMessage);
            Assert.AreEqual("/VM_argument/IntProp", modelState.Errors[0].PropertyPath);
        }

        [TestMethod]
        public async Task Validation_AutomaticInstance()
        {
            var vm = new ViewModelInstance { Property = "test" };
            var plan = CreatePlan(() => vm.ValidatedStaticCommand(null));
            var modelState = await InvokeExpectingErrors(plan, (vm, "/VM_this"), (new TestViewModel(), "/VM_argument"));
            Assert.AreEqual(1, modelState.Errors.Count, $"Unexpected errors: {string.Join(", ", modelState.Errors)}");
            Assert.IsTrue(modelState.Errors[0].IsResolved);
            Assert.AreEqual("automatic-error", modelState.Errors[0].ErrorMessage);
            Assert.AreEqual("/VM_this/Property", modelState.Errors[0].PropertyPath);
        }

        [TestMethod]
        public async Task Validation_AutomaticArgument()
        {
            var vm = new ViewModelInstance { Property = "test" };
            var plan = CreatePlan(() => AutomaticValidation(null, ""));
            var modelState = await InvokeExpectingErrors(plan, (vm, "/VM"), ("test", "/Argument"));
            Assert.AreEqual(2, modelState.Errors.Count, $"Unexpected errors: {string.Join(", ", modelState.Errors)}");
            Assert.IsTrue(modelState.Errors[0].IsResolved);
            Assert.IsTrue(modelState.Errors[1].IsResolved);
            Assert.AreEqual("automatic-error", modelState.Errors[0].ErrorMessage);
            Assert.AreEqual("automatic-arg-error", modelState.Errors[1].ErrorMessage);
            Assert.AreEqual("/VM/Property", modelState.Errors[0].PropertyPath);
            Assert.AreEqual("/Argument", modelState.Errors[1].PropertyPath);
        }

        public class ViewModelInstance
        {
            [RegularExpressionAttribute("(ab)+", ErrorMessage = "automatic-error")]
            public string Property { get; set; } = "value";

            [AllowStaticCommand(StaticCommandValidation.Automatic)]
            internal void ValidatedStaticCommand(TestViewModel vm)
            {
                var ms = new StaticCommandModelState();
                ms.AddArgumentError(() => vm.IntProp, "manual-error");
                ms.FailOnInvalidModelState();
            }
        }

        [AllowStaticCommand(StaticCommandValidation.Automatic)]
        internal static void AutomaticValidation(ViewModelInstance viewModel, [RegularExpression("ab*c", ErrorMessage = "automatic-arg-error")] string argument)
        {
            var ms = new StaticCommandModelState();
            ms.AddArgumentError(() => viewModel.Property, "manual-error");
            ms.FailOnInvalidModelState();
        }

        [TestMethod]
        public async Task Validation_CustomPrimitives()
        {
            var vm = new TestViewModel { VehicleNumber = new(1) };
            var plan = CreatePlan(() => CustomPrimitivesValidation(vm, new VehicleNumber(1)));
            var modelState = await InvokeExpectingErrors(plan, (vm, "/VM"), (new VehicleNumber(321), "/Argument"));

            Assert.AreEqual(2, modelState.Errors.Count, $"Unexpected errors: {string.Join(", ", modelState.Errors)}");
            Assert.IsTrue(modelState.Errors[0].IsResolved);
            Assert.IsTrue(modelState.Errors[1].IsResolved);
            Assert.AreEqual("The field Value must be between 100 and 999.", modelState.Errors[0].ErrorMessage);
            Assert.AreEqual("Vehicle must have lucky number.", modelState.Errors[1].ErrorMessage);
            Assert.AreEqual("/VM/VehicleNumber", modelState.Errors[0].PropertyPath);
            Assert.AreEqual("/Argument", modelState.Errors[1].PropertyPath);
        }

        [AllowStaticCommand(StaticCommandValidation.Automatic)]
        internal static void CustomPrimitivesValidation(TestViewModel viewModel, [RegularExpression(@"\d\d7", ErrorMessage = "Vehicle must have lucky number.")] VehicleNumber vehicle)
        {
        }
    }
}
