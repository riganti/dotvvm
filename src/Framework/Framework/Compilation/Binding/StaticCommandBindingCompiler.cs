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

        /// Replaces delegate arguments with commandArgs reference
        private static Expression ReplaceCommandArgs(Expression expression) =>
            expression.ReplaceAll(e =>
                e?.GetParameterAnnotation()?.ExtensionParameter is TypeConversion.MagicLambdaConversionExtensionParameter extensionParam ?
                    Expression.Parameter(ResolvedTypeDescriptor.ToSystemType(extensionParam.ParameterType), $"commandArgs['{extensionParam.Identifier}']")
                    .AddParameterAnnotation(new BindingParameterAnnotation(extensionParameter: new JavascriptTranslationVisitor.FakeExtensionParameter(
                        _ => new JsSymbolicParameter(CommandBindingExpression.CommandArgumentsParameter)
                             .Indexer(new JsLiteral(extensionParam.ArgumentIndex))
                    ))) :
                e!
            );

        public JsExpression CompileToJavascript(DataContextStack dataContext, Expression expression)
        {
            expression = ReplaceCommandArgs(expression);

            return TranslateVariableDeclaration(expression, e => CreateCommandExpression(dataContext, e));
        }

        private JsExpression TranslateVariableDeclaration(Expression expression, Func<Expression, JsExpression> core)
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
                            new JavascriptTranslationVisitor.FakeExtensionParameter(_ => new JsSymbolicParameter(tmpVar), v.Name, new ResolvedTypeDescriptor(v.Type))
                        ));
                    }).ToArray()
                );

                return core(replacedVariables);
            }
            else
            {
                return core(expression);
            }
        }

        private JsExpression CreateCommandExpression(DataContextStack dataContext, Expression expression)
        {
            var knockoutContext =
                new JsSymbolicParameter(
                    JavascriptTranslator.KnockoutContextParameter,
                    defaultAssignment: new JsIdentifierExpression("ko").Member("contextFor").Invoke(new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter)).FormatParametrizedScript()
                );

            var currentContextVariable = new JsTemporaryVariableParameter(knockoutContext, preferredName: "cx");
            var currentViewModelVariable = new JsTemporaryVariableParameter(new JsSymbolicParameter(JavascriptTranslator.KnockoutViewModelParameter), preferredName: "vm");
            var senderVariable = new JsTemporaryVariableParameter(new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter), preferredName: "sender");

            var invocationRewriter = new InvocationRewriterExpressionVisitor();
            expression = invocationRewriter.Visit(expression);

            var rewriter = new TaskSequenceRewriterExpressionVisitor();
            expression = rewriter.Visit(expression);

            var jsExpression = javascriptTranslator.CompileToJavascript(expression, dataContext, preferUsingState: true, isRootAsync: true);
            return (JsExpression)jsExpression.AssignParameters(symbol =>
                symbol == JavascriptTranslator.KnockoutContextParameter ? currentContextVariable.ToExpression() :
                symbol == JavascriptTranslator.KnockoutViewModelParameter ? currentViewModelVariable.ToExpression() :
                symbol == CommandBindingExpression.SenderElementParameter ? senderVariable.ToExpression() :
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
