using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.CodeAnalysis;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public class UnsupportedCallSiteCheckingVisitor : ResolvedControlTreeVisitor
    {
        class ExpressionInspectingVisitor : ExpressionVisitor
        {
            public event Action<MethodInfo>? InvalidCallSiteDetected;

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.IsDefined(typeof(UnsupportedCallSiteAttribute)))
                {
                    var callSiteAttr = node.Method.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(UnsupportedCallSiteAttribute))!;
                    if (callSiteAttr.ConstructorArguments.Any() && callSiteAttr.ConstructorArguments.First().Value is int type && type == (int)CallSiteType.ServerSide)
                        InvalidCallSiteDetected?.Invoke(node.Method);
                }

                return base.VisitMethodCall(node);
            }
        }

        public override void VisitBinding(ResolvedBinding binding)
        {
            base.VisitBinding(binding);
            if (binding.Binding is not ResourceBindingExpression and not CommandBindingExpression)
                return;

            var expressionVisitor = new ExpressionInspectingVisitor();
            expressionVisitor.InvalidCallSiteDetected += method =>
                binding.DothtmlNode?.AddWarning($"Evaluation of method \"{method.Name}\" on server-side may yield unexpected results.");

            expressionVisitor.Visit(binding.Expression);
        }
    }
}
