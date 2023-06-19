using System;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace DotVVM.Framework.Compilation.Binding
{
    /// <summary> Transforms JsExpressions returning a value into JsExpressions returns the validation path of that value </summary>
    public class ValidationPathFormatter
    {
        readonly IViewModelSerializationMapper mapper;
        readonly JavascriptTranslator javascriptTranslator;

        public ValidationPathFormatter(
            IViewModelSerializationMapper mapper,
            JavascriptTranslator javascriptTranslator
        )
        {
            this.mapper = mapper;
            this.javascriptTranslator = javascriptTranslator;
        }

        private bool IsNull([NotNullWhen(false)] JsNode? expr) =>
            expr is null or JsLiteral { Value: null };

        public JsExpression? GetValidationPath(
            Expression expr,
            DataContextStack dataContext,
            Func<Expression, JsExpression?>? baseFormatter = null)
        {
            // TODO: handle lambda arguments
            // TODO: propagate errors into block variables
            expr = ExpressionHelper.UnwrapPassthroughOperations(expr);

            var baseFmt = baseFormatter?.Invoke(expr);
            if (baseFmt is {})
                return baseFmt;

            if (expr.GetParameterAnnotation() is {} annotation)
            {
                if (annotation.ExtensionParameter is not null)
                    return null;

                var parentIndex = dataContext.EnumerableItems().ToList().IndexOf(annotation.DataContext.NotNull());

                if (parentIndex < 0)
                    throw new InvalidOperationException($"DataContext parameter is invalid. Current data context is {dataContext}, the parameter is not one of the ancestors: {annotation.DataContext}");

                if (parentIndex == 0)
                    return new JsLiteral(".");
                else
                    return new JsLiteral(string.Join("/", Enumerable.Repeat("..", parentIndex)));
            }

            switch (expr)
            {
                case MemberExpression m when m.Expression is {}: {
                    var targetPath = GetValidationPath(m.Expression, dataContext, baseFormatter);
                    if (IsNull(targetPath))
                        return targetPath;

                    var typeMap = mapper.GetMap(m.Member.DeclaringType!);
                    var property = typeMap.Properties.FirstOrDefault(p => p.PropertyInfo == m.Member);

                    if (property is null)
                        return JsLiteral.Null.CommentBefore($"{m.Member.Name} is not mapped");

                    return AppendPaths(targetPath, property.Name);
                }
                case ConditionalExpression conditional: {
                    var truePath = GetValidationPath(conditional.IfTrue, dataContext, baseFormatter);
                    var falsePath = GetValidationPath(conditional.IfFalse, dataContext, baseFormatter);

                    if (IsNull(truePath) || IsNull(falsePath))
                        return truePath ?? falsePath;

                    if (truePath is JsLiteral { Value: var trueValue } && falsePath is JsLiteral { Value: var falseValue } && object.Equals(trueValue, falseValue))
                        return truePath;

                    JsExpression condition;
                    try
                    {
                        condition = this.javascriptTranslator.CompileToJavascript(conditional.Test, dataContext);
                    }
                    catch
                    {
                        return JsLiteral.Null.CommentBefore("Unsupported condition");
                    }

                    return new JsConditionalExpression(
                        condition,
                        truePath,
                        falsePath
                    );
                }
                case IndexExpression { Object: {} } index: {
                    var targetPath = GetValidationPath(index.Object, dataContext, baseFormatter);
                    if (IsNull(targetPath))
                        return targetPath;
                    if (index.Arguments.Count != 1 || !index.Arguments.Single().Type.IsNumericType())
                        return JsLiteral.Null.CommentBefore("Unsupported index");

                    var indexPath = this.javascriptTranslator.CompileToJavascript(index.Arguments.Single(), dataContext);
                    return AppendPaths(targetPath, indexPath);
                }
                case BinaryExpression b when b.NodeType == ExpressionType.ArrayIndex: {
                    var targetPath = GetValidationPath(b.Left, dataContext, baseFormatter);
                    if (IsNull(targetPath))
                        return targetPath;
                    var indexPath = this.javascriptTranslator.CompileToJavascript(b.Right, dataContext);
                    return AppendPaths(targetPath, indexPath);
                }
                case ConstantExpression:
                    return JsLiteral.Null; // no need to explain the reason
                default:
                    return JsLiteral.Null.CommentBefore($"Expression {expr.NodeType} ({expr}) isn't supported");
            }
        }

        static JsExpression AppendPaths(JsExpression left, JsExpression right)
        {
            if (left is JsLiteral { Value: null })
                return left;
            if (right is JsLiteral { Value: null })
                return right;

            if (right is JsLiteral { Value: not null } rightLiteral)
                return AppendPaths(left, rightLiteral.Value.ToString()!);
            else if (left is JsLiteral { Value: "." })
                return right;
            else
                return new JsBinaryExpression(StringAppend(left, "/"), BinaryOperatorType.Plus, right);
        }

        static JsExpression AppendPaths(JsExpression left, string right)
        {
            if (left is JsLiteral { Value: "." } l)
                return new JsLiteral(right);
            return StringAppend(left, "/" + right);
        }

        static JsExpression StringAppend(JsExpression left, string right)
        {
            if (left is JsLiteral l && l.Value is string s)
                return new JsLiteral(s + right);
            else if (left is JsBinaryExpression { OperatorString: "+", Right: JsLiteral { Value: string str2 } } binary)
                return new JsBinaryExpression(left, BinaryOperatorType.Plus, new JsLiteral(str2 + right));
            else
                return new JsBinaryExpression(left, BinaryOperatorType.Plus, new JsLiteral(right));
        }
    }
}
