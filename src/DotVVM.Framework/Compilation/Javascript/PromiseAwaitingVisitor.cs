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
                throw new Exception($"Can not use async expression in synchronous context. The expression: {expression.FormatScript(isDebugString: true)}");
            }
            if (parentFunction is { IsAsync: false })
            {
                if (onlyCheck)
                    return false;
                throw new Exception($"Can not use async expression in non-async function. The expression: {expression.FormatScript(isDebugString: true)}; The function: {parentFunction.FormatScript(isDebugString: true)}");
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
                if (isGetterOptional)
                    getPromise = e => e;

                if (AssertIsInAsyncFunction(expr, onlyCheck: isOptionalAwait))
                {
                    expr.ReplaceWith(e => AddAnnotations(getPromise(e).Await().WithAnnotations(resultAnnotations), expr));
                }
            }
        }

        private bool IsAlreadyAwaited(JsNode node) =>
            node.SatisfyResultCondition(n => n.Parent is JsUnaryExpression { Operator: UnaryOperatorType.Await });

        /// Adds annotations about the value of the expression (if it may be null, type of the expression, ...)
        private JsExpression AddAnnotations(JsExpression expr, JsExpression originalNode) =>
            expr.WithAnnotation(originalNode.Annotation<ResultIsObservableAnnotation>(), append: false)
                .WithAnnotation(originalNode.Annotation<ResultIsObservableArrayAnnotation>(), append: false)
                .WithAnnotation(originalNode.Annotation<ResultMayBeObservableAnnotation>(), append: false)
                .WithAnnotation(originalNode.Annotation<VMPropertyInfoAnnotation>(), append: false)
                .WithAnnotation(originalNode.Annotation<ViewModelInfoAnnotation>(), append: false)
                .WithAnnotation(originalNode.Annotation<MayBeNullAnnotation>(), append: false);
    }
}
