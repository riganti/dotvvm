using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    /// <summary> Add await operator around every expression that returns a promise, if the function is async </summary>
    public class PromiseAwaitingVisitor : JsNodeVisitor
    {
        /// <summary> Says if the root expression allows await operators </summary>
        readonly bool isRootAsync;
        public PromiseAwaitingVisitor(bool isRootAsync)
        {
            this.isRootAsync = isRootAsync;
        }

        bool AssertIsInAsyncFunction(JsExpression expression, bool onlyCheck)
        {
            var parentFunction = expression.Ancestors.OfType<JsFunctionExpression>().FirstOrDefault();
            if (parentFunction is null && !isRootAsync)
            {
                if (onlyCheck)
                    return false;
                throw new Exception($"Cannot use async expression in synchronous context. The expression: {expression.FormatScript(isDebugString: true)}");
            }
            if (parentFunction is { IsAsync: false })
            {
                if (onlyCheck)
                    return false;
                throw new Exception($"Cannot use async expression in non-async function. The expression: {expression.FormatScript(isDebugString: true)}; The function: {parentFunction.FormatScript(isDebugString: true)}");
            }
            return true;
        }

        protected override void DefaultVisit(JsNode node)
        {
            base.DefaultVisit(node);

            if (node is JsExpression expr)
                HandleExpression(expr);
        }

        private void HandleExpression(JsExpression expr)
        {
            if (expr.Annotation<ResultIsPromiseAnnotation>() is {
                    GetPromiseFromExpression: var getPromise,
                    ResultAnnotations: var resultAnnotations,
                    IsOptionalAwait: var isOptionalAwait,
                    IsPromiseGetterOptional: var isGetterOptional
                } && !IsAlreadyAwaited(expr))
            {
                if (AssertIsInAsyncFunction(expr, onlyCheck: isOptionalAwait))
                {
                    expr.ReplaceWith(e => {
                        var promiseExpr = isGetterOptional ? e : getPromise(e);
                        return AddAnnotations(promiseExpr.Await().WithAnnotations(resultAnnotations), promiseExpr);
                    });
                }
            }
        }

        private bool IsAlreadyAwaited(JsNode node) =>
            node.SatisfyResultCondition(n => n.Parent is JsUnaryExpression { Operator: UnaryOperatorType.Await });

        /// Adds annotations about the value of the expression (if it may be null, type of the expression, ...)
        private JsExpression AddAnnotations(JsExpression expr, JsExpression originalNode) =>
            expr.WithConditionalAnnotation(originalNode.HasAnnotation(ResultIsObservableAnnotation.Instance), ResultIsObservableAnnotation.Instance, append: false)
                .WithConditionalAnnotation(originalNode.HasAnnotation(ResultIsObservableArrayAnnotation.Instance), ResultIsObservableArrayAnnotation.Instance, append: false)
                .WithConditionalAnnotation(originalNode.HasAnnotation(ResultMayBeObservableAnnotation.Instance), ResultMayBeObservableAnnotation.Instance, append: false)
                .WithAnnotation(originalNode.Annotation<VMPropertyInfoAnnotation>(), append: false)
                .WithAnnotation(originalNode.Annotation<ViewModelInfoAnnotation>(), append: false)
                .WithConditionalAnnotation(originalNode.HasAnnotation(MayBeNullAnnotation.Instance), MayBeNullAnnotation.Instance, append: false);
    }
}
