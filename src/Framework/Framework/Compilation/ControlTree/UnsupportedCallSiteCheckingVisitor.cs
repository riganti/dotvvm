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
                var callSiteAttr = node.Method.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(UnsupportedCallSiteAttribute));
                if (callSiteAttr is not null && callSiteAttr.ConstructorArguments.Any())
                {
                    if (callSiteAttr.ConstructorArguments.First().Value is int type && type == (int)CallSiteType.ServerSide)
                        InvalidCallSiteDetected?.Invoke(node.Method);
                }

                return base.VisitMethodCall(node);
            }
        }

        public override void VisitBinding(ResolvedBinding binding)
        {
            base.VisitBinding(binding);
            if (binding.Binding is not ResourceBindingExpression)
                return;

            var expressionVisitor = new ExpressionInspectingVisitor();
            expressionVisitor.InvalidCallSiteDetected += method =>
                binding.DothtmlNode?.AddWarning($"Evaluation of method \"{method.Name}\" on server-side may yield unexpected results.");

            expressionVisitor.Visit(binding.Expression);
        }
    }
}
