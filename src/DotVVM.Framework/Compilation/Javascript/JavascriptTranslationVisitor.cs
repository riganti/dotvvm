﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.HelperNamespace;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class JavascriptTranslationVisitor
    {
        private readonly IJavascriptMethodTranslator Translator;

        public DataContextStack DataContext { get; }

        private readonly Dictionary<DataContextStack, int> ContextMap;
        public bool WriteUnknownParameters { get; set; } = true;
        public JavascriptTranslationVisitor(DataContextStack dataContext, IJavascriptMethodTranslator translator)
        {
            this.ContextMap = dataContext.EnumerableItems().Select((a, i) => (a, i)).ToDictionary(a => a.Item1, a => a.Item2);
            this.DataContext = dataContext;
            this.Translator = translator;
        }
        public JsExpression Translate(Expression expression)
        {
            if (expression.GetParameterAnnotation() is BindingParameterAnnotation annotation)
                return TranslateParameter(expression, annotation);

            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return TranslateConstant((ConstantExpression)expression);

                case ExpressionType.Call:
                    return TranslateMethodCall((MethodCallExpression)expression);

                case ExpressionType.MemberAccess:
                    return TranslateMemberAccess((MemberExpression)expression);

                case ExpressionType.Parameter:
                    return TranslateParameter((ParameterExpression)expression);

                case ExpressionType.Conditional:
                    return TranslateConditional((ConditionalExpression)expression);

                case ExpressionType.Index:
                    return TranslateIndex((IndexExpression)expression);

                case ExpressionType.Assign:
                    return TranslateAssign((BinaryExpression)expression);

                case ExpressionType.Lambda:
                    return TranslateLambda((LambdaExpression)expression);

                case ExpressionType.Block:
                    return TranslateBlock((BlockExpression)expression);
                
                case ExpressionType.Default:
                    return TranslateDefault((DefaultExpression)expression);

                case ExpressionType.Invoke:
                    return TranslateInvoke((InvocationExpression)expression);
            }
            if (expression is BinaryExpression)
            {
                return TranslateBinary((BinaryExpression)expression);
            }
            else if (expression is UnaryExpression)
            {
                return TranslateUnary((UnaryExpression)expression);
            }

            throw new NotSupportedException($"The expression type {expression.NodeType} can not be translated to Javascript!");
        }

        private JsExpression TranslateInvoke(InvocationExpression expression)
        {
            // just invoke the function
            return Translate(expression.Expression).Invoke(expression.Arguments.Select(Translate));
        }

        private Expression ReplaceVariables(Expression node, IReadOnlyList<ParameterExpression> variables, object[] args)
        {
            return ExpressionUtils.Replace(Expression.Lambda(node, variables), args.Zip(variables, (o, a) => Expression.Parameter(a.Type, a.Name).AddParameterAnnotation(
                new BindingParameterAnnotation(extensionParameter: new FakeExtensionParameter(_ => new JsSymbolicParameter(o), a.Name, new ResolvedTypeDescriptor(a.Type)))
            )).ToArray());
        }

        public JsExpression TranslateLambda(LambdaExpression expression)
        {
            var args = expression.Parameters.Select(_ => new object()).ToArray();
            var (body, additionalVariables, additionalVarNames) = TranslateLambdaBody(ReplaceVariables(expression.Body, expression.Parameters, args));
            var usedNames = new HashSet<string>(body.DescendantNodesAndSelf().OfType<JsIdentifierExpression>().Select(i => i.Identifier));
            var argsNames = expression.Parameters.Select(p => JsTemporaryVariableResolver.GetNames(p.Name).First(usedNames.Add)).ToArray();
            additionalVarNames = additionalVarNames.Select(p => JsTemporaryVariableResolver.GetNames(p).First(usedNames.Add)).ToArray();
            foreach (var symArg in body.DescendantNodesAndSelf().OfType<JsSymbolicParameter>())
            {
                var aIndex = Array.IndexOf(args, symArg.Symbol);
                if (aIndex >= 0) symArg.ReplaceWith(new JsIdentifierExpression(argsNames[aIndex]).WithAnnotations(symArg.Annotations).WithAnnotation(ResultMayBeObservableAnnotation.Instance, append: false));
                aIndex = Array.IndexOf(additionalVariables, symArg.Symbol);
                if (aIndex >= 0) symArg.ReplaceWith(new JsIdentifierExpression(additionalVarNames[aIndex]));
            }
            return new JsFunctionExpression(
                argsNames.Concat(additionalVarNames).Select(n => new JsIdentifier(n)),
                body is JsBlockStatement block ? block :
                body is JsStatement statement ? new JsBlockStatement(statement) :
                body is JsExpression bodyExpression ? new JsBlockStatement(new JsReturnStatement(bodyExpression)) :
                throw new NotSupportedException()
            );
        }
        (JsNode node, object[] variables, string[] variableNames) TranslateLambdaBody(Expression expression)
        {
            if (expression is BlockExpression block)
            {
                var args = block.Variables.Select(_ => new object()).ToArray();
                var expressions = block.Expressions.Select(s => Translate(ReplaceVariables(s, block.Variables, args))).ToArray();
                return (
                    new JsBlockStatement(
                        expressions.Take(expressions.Length - 1).Select(e => (JsStatement)new JsExpressionStatement(e)).Concat(new [] { new JsReturnStatement(expressions.Last()) }).ToArray()
                    ),
                    args,
                    block.Variables.Select(a => a.Name).ToArray()
                );
            }
            else return (Translate(expression), new object[0], new string[0]);
        }


        public JsExpression TranslateBlock(BlockExpression expression)
        {
            if (expression.Variables.Any())
            {
                return TranslateLambda(Expression.Lambda(expression)).Invoke();
            }
            else
            {
                var body = expression.Expressions;
                if (body.Count == 1) return Translate(body[0]);
                return body.Select(Translate).Aggregate(
                    (a, b) => (JsExpression)new JsBinaryExpression(a, BinaryOperatorType.Sequence, b));
            }
        }

        public JsExpression TranslateAssign(BinaryExpression expression)
        {
            var property = expression.Left as MemberExpression;
            if (property != null)
            {
                var target = Translate(property.Expression);
                var value = Translate(expression.Right);
                return TryTranslateMethodCall((property.Member as PropertyInfo)?.SetMethod, property.Expression, new[] { expression.Right }) ??
                    SetProperty(target, property.Member as PropertyInfo, value);
            }
            else if (expression.Left.GetParameterAnnotation() is BindingParameterAnnotation annotation)
            {
                if (annotation.ExtensionParameter == null) throw new NotSupportedException($"Can not assign to data context parameter {expression.Left}");
                return new JsAssignmentExpression(
                    TranslateParameter(expression.Left, annotation),
                    Translate(expression.Right)
                );
            }
            throw new NotSupportedException($"Can not assign expression of type {expression.Left.NodeType}!");
        }

        private JsExpression SetProperty(JsExpression target, PropertyInfo property, JsExpression value) =>
            new JsAssignmentExpression(TranslateViewModelProperty(target, property), value);

        public JsExpression TranslateConditional(ConditionalExpression expression) =>
            new JsConditionalExpression(
                Translate(expression.Test),
                Translate(expression.IfTrue),
                Translate(expression.IfFalse));

        public JsExpression TranslateIndex(IndexExpression expression, bool setter = false)
        {
            var target = Translate(expression.Object);
            var args = expression.Arguments.Select(Translate).ToArray();
            var method = setter ? expression.Indexer.SetMethod : expression.Indexer.GetMethod;

            var result = TryTranslateMethodCall(method, expression.Object, expression.Arguments.ToArray());
            if (result != null) return result;
            return BuildIndexer(target, args.Single(), expression.Indexer);
        }

        public static JsExpression BuildIndexer(JsExpression target, JsExpression index, MemberInfo member) =>
            target.Indexer(index).WithAnnotation(new VMPropertyInfoAnnotation { MemberInfo = member });

        public JsExpression TranslateParameter(Expression expression, BindingParameterAnnotation annotation)
        {
            JsExpression getDataContext(int parentContexts)
            {
                JsExpression context = new JsSymbolicParameter(JavascriptTranslator.KnockoutContextParameter);
                for (var i = 0; i < parentContexts; i++)
                    context = context.Member("$parentContext");
                return context;
            }
            int getContextSteps(DataContextStack item) =>
                item == null ? 0 : ContextMap[item];
            JsExpression contextParameter(string name, int parentContexts, Type type) =>
                getDataContext(parentContexts).Member(name).WithAnnotation(new ViewModelInfoAnnotation(type));

            if (annotation.ExtensionParameter != null)
            {
                return annotation.ExtensionParameter.GetJsTranslation(getDataContext(getContextSteps(annotation.DataContext)))
                    .WithAnnotation(new ViewModelInfoAnnotation(annotation.ExtensionParameter.ParameterType.Apply(ResolvedTypeDescriptor.ToSystemType), extensionParameter: annotation.ExtensionParameter), append: false);
            }
            else
            {
                var index = getContextSteps(annotation.DataContext);
                if (index == 0)
                    return new JsSymbolicParameter(JavascriptTranslator.KnockoutViewModelParameter).WithAnnotation(new ViewModelInfoAnnotation(expression.Type));
                else if (index == 1)
                    return contextParameter("$parent", 0, expression.Type);
                else if (ContextMap.Count == index + 1)
                    return contextParameter("$root", 0, expression.Type);
                else return new JsSymbolicParameter(JavascriptTranslator.KnockoutContextParameter)
                        .Member("$parents").Indexer(new JsLiteral(index - 1))
                        .WithAnnotation(new ViewModelInfoAnnotation(expression.Type));
            }
        }

        public JsExpression TranslateParameter(ParameterExpression expression)
        {
            if (WriteUnknownParameters && !string.IsNullOrEmpty(expression.Name)) return new JsIdentifierExpression(expression.Name);
            else throw new NotSupportedException($"Can't translate parameter '{expression}' to Javascript.");
        }

        public JsLiteral TranslateDefault(DefaultExpression expression) =>
            new JsLiteral(expression.Type.IsValueType && expression.Type != typeof(void) ? 
                          Activator.CreateInstance(expression.Type) :
                          null)
            .WithAnnotation(new ViewModelInfoAnnotation(expression.Type));

        public JsLiteral TranslateConstant(ConstantExpression expression) =>
            new JsLiteral(expression.Value).WithAnnotation(new ViewModelInfoAnnotation(expression.Type));

        public JsExpression TranslateMethodCall(MethodCallExpression expression)
        {
            var thisExpression = expression.Object == null ? null : Translate(expression.Object);
            var args = expression.Arguments.Select(Translate).ToArray();

            var result = TryTranslateMethodCall(expression.Method, expression.Object, expression.Arguments.ToArray());
            if (result == null)
                throw new NotSupportedException($"Method { expression.Method.DeclaringType.Name }.{ expression.Method.Name } can not be translated to Javascript");
            return result;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public JsExpression TranslateBinary(BinaryExpression expression)
        {
            var left = Translate(expression.Left);
            var right = Translate(expression.Right);
            var method = expression.Method;
            if (method != null)
            {
                var mTranslate = TryTranslateMethodCall(expression.Method, null, new[] { expression.Left, expression.Right });
                if (mTranslate != null) return mTranslate;
            }
            BinaryOperatorType op;
            switch (expression.NodeType)
            {
                case ExpressionType.Equal: op = BinaryOperatorType.Equal; break;
                case ExpressionType.NotEqual: op = BinaryOperatorType.NotEqual; break;
                case ExpressionType.AndAlso: op = BinaryOperatorType.ConditionalAnd; break;
                case ExpressionType.OrElse: op = BinaryOperatorType.ConditionalOr; break;
                case ExpressionType.GreaterThan: op = BinaryOperatorType.Greater; break;
                case ExpressionType.LessThan: op = BinaryOperatorType.Less; break;
                case ExpressionType.GreaterThanOrEqual: op = BinaryOperatorType.GreaterOrEqual; break;
                case ExpressionType.LessThanOrEqual: op = BinaryOperatorType.LessOrEqual; break;
                case ExpressionType.AddChecked:
                case ExpressionType.Add: op = BinaryOperatorType.Plus; break;
                case ExpressionType.SubtractChecked:
                case ExpressionType.Subtract: op = BinaryOperatorType.Minus; break;
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.Divide: op = BinaryOperatorType.Divide; break;
                case ExpressionType.Modulo: op = BinaryOperatorType.Modulo; break;
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Multiply: op = BinaryOperatorType.Times; break;
                case ExpressionType.LeftShift: op = BinaryOperatorType.LeftShift; break;
                case ExpressionType.RightShift: op = BinaryOperatorType.UnsignedRightShift; break;
                case ExpressionType.And: op = BinaryOperatorType.BitwiseAnd; break;
                case ExpressionType.Or: op = BinaryOperatorType.BitwiseOr; break;
                case ExpressionType.ExclusiveOr: op = BinaryOperatorType.BitwiseXOr; break;
                case ExpressionType.Coalesce: op = BinaryOperatorType.ConditionalOr; break;
                case ExpressionType.ArrayIndex: return new JsIndexerExpression(left, right);
                default:
                    throw new NotSupportedException($"Unary operator of type { expression.NodeType } is not supported");
            }
            return new JsBinaryExpression(left, op, right);
        }

        public JsExpression TranslateUnary(UnaryExpression expression)
        {
            var method = expression.Method;
            if (method != null)
            {
                var mTranslate = TryTranslateMethodCall(expression.Method, null, new[] { expression.Operand });
                if (mTranslate != null) return mTranslate;
            }
            var operand = Translate(expression.Operand);
            UnaryOperatorType op;
            switch (expression.NodeType)
            {
                case ExpressionType.NegateChecked:
                case ExpressionType.Negate:
                    op = UnaryOperatorType.Minus;
                    break;

                case ExpressionType.UnaryPlus:
                    op = UnaryOperatorType.Plus;
                    break;

                case ExpressionType.Not:
                    if (expression.Operand.Type == typeof(bool))
                        op = UnaryOperatorType.LogicalNot;
                    else op = UnaryOperatorType.BitwiseNot;
                    break;
                case ExpressionType.Convert:
                case ExpressionType.TypeAs:
                    // convert does not make sense in Javascript
                    return operand;

                default:
                    throw new NotSupportedException($"Unary operator of type { expression.NodeType } is not supported");
            }
            return new JsUnaryExpression(op, operand);
        }

        public JsExpression TranslateMemberAccess(MemberExpression expression)
        {
            var getter = (expression.Member as PropertyInfo)?.GetMethod;
            if (expression.Expression == null)
            {
                // static
                return TryTranslateMethodCall(getter, null, new Expression[0]) ??
                    new JsLiteral((
                        ((expression.Member as FieldInfo)?.GetValue(null) ?? (expression.Member as PropertyInfo)?.GetValue(null))));
            }
            else
            {
                return TryTranslateMethodCall(getter, expression.Expression, new Expression[0]) ??
                    TranslateViewModelProperty(Translate(expression.Expression), expression.Member);
            }
        }

        public static JsExpression TranslateViewModelProperty(JsExpression context, MemberInfo propInfo, string name = null) =>
            new JsMemberAccessExpression(context, name ?? propInfo.Name).WithAnnotation(new VMPropertyInfoAnnotation { MemberInfo = propInfo }).WithAnnotation(new ViewModelInfoAnnotation(propInfo.GetResultType()));

        public JsExpression TryTranslateMethodCall(MethodInfo methodInfo, Expression target, IEnumerable<Expression> arguments) =>
            Translator.TryTranslateCall(
                new LazyTranslatedExpression(target, Translate),
                arguments.Select(a => new LazyTranslatedExpression(a, Translate)).ToArray(),
                methodInfo)
                ?.WithAnnotation(new ViewModelInfoAnnotation(methodInfo.ReturnType), append: false);

        public class FakeExtensionParameter: BindingExtensionParameter
        {
            private readonly Func<JsExpression, JsExpression> getJsTranslation;

            public FakeExtensionParameter(Func<JsExpression, JsExpression> getJsTranslation, string identifier = "__", ITypeDescriptor type = null, bool inherit = false): base(identifier, type, inherit)
            {
                this.getJsTranslation = getJsTranslation;
            }

            public override Expression GetServerEquivalent(Expression controlParameter) => throw new NotSupportedException();
            public override JsExpression GetJsTranslation(JsExpression dataContext) => getJsTranslation(dataContext);
        }
    }

    /// Represents an Linq.Expression that is being translated to JsAst.
    public class LazyTranslatedExpression
    {
        private static readonly Lazy<JsExpression> nullLazy = new Lazy<JsExpression>(() => null);
        private readonly Lazy<JsExpression> lazyJsExpression;
        public JsExpression JsExpression() => lazyJsExpression.Value;
        public Expression OriginalExpression { get; }
        public LazyTranslatedExpression(Expression expr, Func<Expression, JsExpression> translateMethod)
        {
            this.OriginalExpression = expr;
            this.lazyJsExpression = expr == null ? nullLazy : new Lazy<JsExpression>(() => translateMethod(expr));
        }
    }
}