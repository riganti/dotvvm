using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Validation;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Hosting
{
    public class StaticCommandExecutor
    {
#pragma warning disable CS0618
        private readonly IStaticCommandServiceLoader serviceLoader;
        private readonly IViewModelProtector viewModelProtector;
        private readonly DotvvmConfiguration configuration;

        public StaticCommandExecutor(IStaticCommandServiceLoader serviceLoader, IViewModelProtector viewModelProtector, DotvvmConfiguration configuration)
        {
            this.serviceLoader = serviceLoader;
            this.viewModelProtector = viewModelProtector;
            this.configuration = configuration;
        }
#pragma warning restore CS0618

        public StaticCommandInvocationPlan DecryptPlan(string encrypted)
        {
            var decrypted = StaticCommandExecutionPlanSerializer.DecryptJson(Convert.FromBase64String(encrypted), viewModelProtector);
            return StaticCommandExecutionPlanSerializer.DeserializePlan(decrypted);
        }
        public Task<object?> Execute(
            StaticCommandInvocationPlan plan,
            IEnumerable<JToken> arguments,
            IEnumerable<string?>? argumentValidationPaths,
            IDotvvmRequestContext context
        ) => Execute(plan, new Queue<JToken>(arguments), argumentValidationPaths is null ? null : new Queue<string?>(argumentValidationPaths), context);

        public async Task<object?> Execute(
            StaticCommandInvocationPlan plan,
            Queue<JToken> arguments,
            Queue<string?>? argumentValidationPaths,
            IDotvvmRequestContext context
        )
        {
            var methodArgs = new List<object?>();
            var methodArgsPaths = argumentValidationPaths is null ? null : new List<string?>();
            foreach (var a in plan.Arguments)
            {
                var (value, path) = a.Type switch {
                    StaticCommandParameterType.Argument =>
                        ((object?)arguments.Dequeue().ToObject((Type)a.Arg!), argumentValidationPaths?.Dequeue()),
                    StaticCommandParameterType.Constant or StaticCommandParameterType.DefaultValue =>
                        (a.Arg, null),
                    StaticCommandParameterType.Inject =>
#pragma warning disable CS0618
                        (serviceLoader.GetStaticCommandService((Type)a.Arg!, context), null),
#pragma warning restore CS0618
                    StaticCommandParameterType.Invocation =>
                        (await Execute((StaticCommandInvocationPlan)a.Arg!, arguments, argumentValidationPaths, context), null),
                    _ => throw new NotSupportedException("" + a.Type)
                };
                methodArgs.Add(value);
                methodArgsPaths?.Add(path);
            }

            try
            {
                var result = plan.Method.Invoke(
                    plan.Method.IsStatic ? null : methodArgs.First(),
                    (plan.Method.IsStatic ? methodArgs : methodArgs.Skip(1)).ToArray());

                if (result is Task task)
                {
                    await task;
                    return TaskUtils.GetResult(task);
                }
                else
                {
                    return result;
                }
            }
            catch (TargetInvocationException ex) when (ex.InnerException is DotvvmInvalidStaticCommandModelStateException innerEx)
            {
                ResolveValidationPaths(innerEx.StaticCommandModelState, plan.Method, methodArgsPaths?.ToArray(), innerEx);
                throw;
            }
            catch (DotvvmInvalidStaticCommandModelStateException ex)
            {
                ResolveValidationPaths(ex.StaticCommandModelState, plan.Method, methodArgsPaths?.ToArray(), ex);
                throw;
            }
        }

        private void ResolveValidationPaths(StaticCommandModelState state, MethodInfo method, string?[]? argumentPaths, DotvvmInvalidStaticCommandModelStateException? innerException)
        {
            var invokedMethodParameters = method.GetParameters();

            foreach (var error in state.Errors.Where(e => !e.IsResolved))
            {
                if (argumentPaths is null)
                    throw new Exception("Could not respond with validation failure because the client did not send validation paths.", innerException);
                if (error.PropertyPathExtractor != null)
                {
                    var path = error.PropertyPathExtractor(configuration);
                    var hasPropertySegment = path.Count(static c => c == '/') >= 2;
                    var name = hasPropertySegment ? path.Substring(0, path.IndexOf('/')) : path;
                    var rest = hasPropertySegment ? path.Substring(name.Length + 1) : string.Empty;
                    error.ArgumentName = name;
                    error.PropertyPath = rest;
                }

                var parameter = invokedMethodParameters.FirstOrDefault(p => p.Name == error.ArgumentName)
                    ?? throw new ArgumentException($"Could not map argument name \"{error.ArgumentName}\" to any parameter of {ReflectionUtils.FormatMethodInfo(method)}.", innerException);

                var argumentIndex = parameter.Position;
                var propertyPath = error.PropertyPath?.Trim('/');
                var argumentPath = argumentPaths![argumentIndex]?.TrimEnd('/');
                if (argumentPath is null)
                    throw new StaticCommandMissingValidationPathException(error, innerException);
                error.PropertyPath = $"{argumentPath}/{propertyPath}".TrimEnd('/');
                error.IsResolved = true;
            }
        }

        record StaticCommandMissingValidationPathException(StaticCommandValidationError ValidationError, Exception? InnerException): RecordExceptions.RecordException(InnerException)
        {
            public override string Message => $"Could not serialize validation error {ValidationError.ArgumentName}/{ValidationError.PropertyPath}, the client did not specify the validation path for this method argument. Make sure that the argument maps directly into a view model property (or use AddRawError to sidestep the automagic mapping in advanced cases).";
        }

        public void DisposeServices(IDotvvmRequestContext context)
        {
#pragma warning disable CS0618
            serviceLoader.DisposeStaticCommandServices(context);
#pragma warning restore CS0618
        }

    }
}
