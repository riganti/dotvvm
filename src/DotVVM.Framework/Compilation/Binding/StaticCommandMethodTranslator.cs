using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Security;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using static DotVVM.Framework.Compilation.Binding.StaticCommandExecutionPlanSerializer;

namespace DotVVM.Framework.Compilation.Binding
{
    public class StaticCommandMethodTranslator : IJavascriptMethodTranslator
    {
        readonly IViewModelProtector protector;

        public StaticCommandMethodTranslator(IViewModelProtector protector)
        {
            this.protector = protector;
        }

        public JsExpression TryTranslateCall(LazyTranslatedExpression context, LazyTranslatedExpression[] arguments, MethodInfo method)
        {
            // throw new Exception($"Method '{methodExpression.Method.DeclaringType.Name}.{methodExpression.Method.Name}' used in static command has to be marked with [AllowStaticCommand] attribute.");
            if (!method.IsDefined(typeof(AllowStaticCommandAttribute)))
                return null;

            var (plan, args) = CreateExecutionPlan(context, arguments, method);
            var encryptedPlan = EncryptJson(SerializePlan(plan), protector).Apply(Convert.ToBase64String);

            var resultTypeAnn = new ViewModelInfoAnnotation(
                ReflectionUtils.UnwrapTaskType(method.ReturnType),
                containsObservables: false
            );

            return new JsIdentifierExpression("dotvvm").Member("staticCommandPostback")
                .Invoke(new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter), new JsLiteral(encryptedPlan), new JsArrayExpression(args), new JsSymbolicParameter(CommandBindingExpression.PostbackOptionsParameter))
                .WithAnnotation(new StaticCommandInvocationJsAnnotation(plan))
                .WithAnnotation(new ResultIsPromiseAnnotation(e => e, resultTypeAnn))
                .WithAnnotation(resultTypeAnn);
        }

        private (StaticCommandInvocationPlan plan, JsExpression[] clientArgs) CreateExecutionPlan(LazyTranslatedExpression context, LazyTranslatedExpression[] arguments, MethodInfo method)
        {
            var allArguments = (context?.OriginalExpression is null ? new LazyTranslatedExpression[0] : new[] { context }).Concat(arguments).ToArray();
            var clientArgs = new List<JsExpression>();

            var argPlans = allArguments.Select((arg, index) => {
                if (arg.OriginalExpression.GetParameterAnnotation() is BindingParameterAnnotation { ExtensionParameter:  InjectedServiceExtensionParameter service })
                    return new StaticCommandParameterPlan(StaticCommandParameterType.Inject, ResolvedTypeDescriptor.ToSystemType(service.ParameterType));
                else if (arg.OriginalExpression is ConstantExpression constant)
                {
                    if (constant.Value == method.GetParameters()[index - (method.IsStatic ? 0 : 1)].DefaultValue)
                        return new StaticCommandParameterPlan(StaticCommandParameterType.DefaultValue, null);
                    else
                        return new StaticCommandParameterPlan(StaticCommandParameterType.Constant, constant.Value);
                }
                else
                {
                    clientArgs.Add(arg.JsExpression());
                    return new StaticCommandParameterPlan(StaticCommandParameterType.Argument, arg.OriginalExpression.Type);
                }
            }).ToArray();
            return (
                new StaticCommandInvocationPlan(method, argPlans),
                clientArgs.ToArray()
            );
        }

        public sealed class StaticCommandInvocationJsAnnotation
        {
            public MethodInfo Method => Plan.Method;
            public StaticCommandInvocationPlan Plan { get; }
            public StaticCommandInvocationJsAnnotation(StaticCommandInvocationPlan plan)
            {
                this.Plan = plan;
            }
        }
    }
}
