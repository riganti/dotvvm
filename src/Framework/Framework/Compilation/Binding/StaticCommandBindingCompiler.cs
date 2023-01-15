using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using FastExpressionCompiler;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.Compilation.Binding
{
    public class StaticCommandBindingCompiler
    {
        private readonly JavascriptTranslator javascriptTranslator;
        public StaticCommandBindingCompiler(IOptions<JavascriptTranslatorConfiguration> config, IViewModelSerializationMapper serializationMapper, StaticCommandMethodTranslator staticCommandTranslator)
        {
            var configForStaticCommands = new JavascriptTranslatorConfiguration();
            configForStaticCommands.Translators.Add(config.Value);
            configForStaticCommands.Translators.Add(staticCommandTranslator);

            javascriptTranslator = new JavascriptTranslator(configForStaticCommands, serializationMapper);
        }

        public JsExpression CompileToJavascript(DataContextStack dataContext, Expression expression)
        {
            var expressionWithVariableResolved = TranslateVariableDeclaration(expression);
            var jsExpression = CreateCommandExpression(dataContext, expressionWithVariableResolved);

            if (jsExpression is JsArrowFunctionExpression wrapperFunction)
            {
                // the function expects command variables
                var args = wrapperFunction.Parameters.Select((p, i) =>
                    new JsTemporaryVariableParameter(
                        CommandBindingExpression.CommandArgumentsParameter.ToExpression()
                            .Indexer(new JsLiteral(i)),
                        preferredName: p.Name
                    ).ToExpression());
                jsExpression = wrapperFunction.SubstituteArguments(args.ToArray());
            }

            return jsExpression;
        }

        private Expression TranslateVariableDeclaration(Expression expression)
        {
            expression = VariableHoistingVisitor.HoistVariables(expression);
            if (expression is BlockExpression block && block.Variables.Any())
            {
                var realBlock = block.Update(Enumerable.Empty<ParameterExpression>(), block.Expressions);

                var variables = block.Variables;
                var replacedVariables = ExpressionUtils.Replace(
                    Expression.Lambda(realBlock, variables),
                    variables.Select(v => {
                        var tmpVar = new JsTemporaryVariableParameter();
                        return Expression.Parameter(v.Type, v.Name).AddParameterAnnotation(new BindingParameterAnnotation(extensionParameter:
                            new JavascriptTranslationVisitor.FakeExtensionParameter(_ => new JsSymbolicParameter(tmpVar), v.Name!, new ResolvedTypeDescriptor(v.Type))
                        ));
                    }).ToArray()
                );

                return replacedVariables;
            }
            else
            {
                return expression;
            }
        }

        private JsExpression CreateCommandExpression(DataContextStack dataContext, Expression expression)
        {
            var knockoutContext =
                new JsSymbolicParameter(
                    JavascriptTranslator.KnockoutContextParameter,
                    defaultAssignment: new JsIdentifierExpression("ko").Member("contextFor").Invoke(new JsSymbolicParameter(JavascriptTranslator.CurrentElementParameter)).FormatParametrizedScript()
                );

            var currentContextVariable = new JsTemporaryVariableParameter(knockoutContext, preferredName: "cx");
            var currentViewModelVariable = new JsTemporaryVariableParameter(new JsSymbolicParameter(JavascriptTranslator.KnockoutViewModelParameter), preferredName: "vm");
            var senderVariable = new JsTemporaryVariableParameter(new JsSymbolicParameter(JavascriptTranslator.CurrentElementParameter), preferredName: "sender");

            var invocationRewriter = new InvocationRewriterExpressionVisitor();
            expression = invocationRewriter.Visit(expression);

            var rewriter = new TaskSequenceRewriterExpressionVisitor();
            expression = rewriter.Visit(expression);

            var jsExpression = javascriptTranslator.CompileToJavascript(expression, dataContext, preferUsingState: true, isRootAsync: true);
            return (JsExpression)jsExpression.AssignParameters(symbol =>
                symbol == JavascriptTranslator.KnockoutContextParameter ? currentContextVariable.ToExpression() :
                symbol == JavascriptTranslator.KnockoutViewModelParameter ? currentViewModelVariable.ToExpression() :
                symbol == JavascriptTranslator.CurrentElementParameter ? senderVariable.ToExpression() :
                default
            );
        }
    }

    public class StaticCommandInvocationPlan
    {
        public MethodInfo Method { get; }
        public StaticCommandParameterPlan[] Arguments { get; }
        public StaticCommandInvocationPlan(MethodInfo method, StaticCommandParameterPlan[] args)
        {
            this.Method = method;
            this.Arguments = args;
        }

        public IEnumerable<MethodInfo> GetAllMethods()
        {
            yield return Method;
            foreach (var arg in Arguments)
                if (arg.Type == StaticCommandParameterType.Invocation)
                    foreach (var r in arg.Arg!.CastTo<StaticCommandInvocationPlan>().GetAllMethods())
                        yield return r;
        }

        public override string ToString()
        {
            var args = Arguments.Select(arg => arg.ToString()).StringJoin(", ");
            return $"{Method.DeclaringType.ToCode(stripNamespace: true)}{Method.Name}({args})";
        }
    }

    public class StaticCommandParameterPlan
    {
        public StaticCommandParameterType Type { get; }
        public object? Arg { get; }

        public StaticCommandParameterPlan(StaticCommandParameterType type, object? arg)
        {
            this.Type = type;
            this.Arg = arg;
        }

        public override string ToString() =>
            Type switch {
                StaticCommandParameterType.Constant => Arg?.ToString() ?? "null",
                StaticCommandParameterType.DefaultValue => "default",
                StaticCommandParameterType.Argument => "?",
                StaticCommandParameterType.Inject => $"service({((Type)Arg!).ToCode(stripNamespace: true)})",
                StaticCommandParameterType.Invocation => Arg!.ToString()!,
                _ => "...invalid argument..."
            };
    }
    public enum StaticCommandParameterType : byte
    {
        Argument,
        Inject,
        Constant,
        DefaultValue,
        Invocation
    }
}
