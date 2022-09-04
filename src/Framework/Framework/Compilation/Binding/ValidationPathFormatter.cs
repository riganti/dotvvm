using System;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Compilation.Binding
{
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

        public JsExpression? GetValidationPath(
            Expression expr,
            DataContextStack dataContext,
            Func<Expression, JsExpression?>? baseFormatter = null)
        {
            // TODO: handle lambda arguments
            // TODO: propagate errors into block variables
            expr = ExpressionHelper.UnwrapPassthroughOperations(expr);

            var baseFmt = baseFormatter?.Invoke(expr);
            if (baseFmt is not null)
                return baseFmt;

            if (expr.GetParameterAnnotation() is {} annotation)
            {
                if (annotation.ExtensionParameter is not null)
                    return null;

                var parentIndex = dataContext.EnumerableItems().ToList().IndexOf(annotation.DataContext.NotNull());

                if (parentIndex < 0)
                    throw new InvalidOperationException($"DataContext parameter is invalid. Current data context is {dataContext}, the parameter is not one of the ancestors: {annotation.DataContext}");

                return new JsLiteral(string.Join("/", Enumerable.Repeat("..", parentIndex)));
            }

            switch (expr)
            {
                case MemberExpression m when m.Expression is {}: {
                    var targetPath = GetValidationPath(m.Expression, dataContext, baseFormatter);
                    if (targetPath is null)
                        return null;

                    var typeMap = mapper.GetMap(m.Member.DeclaringType!);
                    var property = typeMap.Properties.FirstOrDefault(p => p.PropertyInfo == m.Member);

                    if (property is null)
                        return null;

                    return stringAppend(targetPath, "/" + property.Name);
                }
                case ConditionalExpression conditional: {
                    var truePath = GetValidationPath(conditional.IfTrue, dataContext, baseFormatter);
                    var falsePath = GetValidationPath(conditional.IfFalse, dataContext, baseFormatter);

                    if (truePath is null || falsePath is null)
                        return null;

                    return new JsConditionalExpression(
                        this.javascriptTranslator.CompileToJavascript(conditional.Test, dataContext),
                        truePath,
                        falsePath
                    );
                }
                case IndexExpression index: {
                    var targetPath = GetValidationPath(index.Object, dataContext, baseFormatter);
                    if (targetPath is null || index.Arguments.Count != 1 || !index.Arguments.Single().Type.IsNumericType())
                        return null;

                    var indexPath = this.javascriptTranslator.CompileToJavascript(index.Arguments.Single(), dataContext);
                    if (indexPath is JsLiteral { Value: not null } indexLiteral)
                        return stringAppend(targetPath, "/" + indexLiteral.Value!.ToString());
                    else
                        return new JsBinaryExpression(stringAppend(targetPath, "/"), BinaryOperatorType.Plus, indexPath);
                }
                default:
                    return null;
            }
        }

        static JsExpression stringAppend(JsExpression left, string right)
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
