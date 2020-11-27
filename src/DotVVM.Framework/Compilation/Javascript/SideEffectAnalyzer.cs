using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Javascript
{
    public static class SideEffectAnalyzer
    {
        /// Returns a <see cref="ExpressionMutationList" /> that contains all possible mutations done by the specified JsNodes
        public static ExpressionMutationList GetPossibleMutations(params JsNode[] nodes)
        {
            var result = new ExpressionMutationList();
            foreach(var node in nodes)
                ComputePossibleMutations(result, node);
            return result;
        }
        static bool isNameOverriden(object name, JsNode n, JsNode rootNode)
        {
            if (!(name is string)) return false;
            while (n != null && n != rootNode)
            {
                if (n is JsFunctionExpression fExpr && fExpr.Parameters.Any(p => p.Name == (string)name))
                    return true;
                n = n.Parent;
            }
            return false;
        }
        static IEnumerable<object> getAssignedPath(JsExpression expression, JsNode rootNode)
        {
            if (expression is JsIdentifierExpression identifier && !isNameOverriden(identifier.Identifier, identifier, rootNode))
                return new [] { identifier.Identifier };
            else if (expression is JsSymbolicParameter symbol)
                return new [] { symbol.Symbol };
            else if (expression is JsMemberAccessExpression member)
            {
                var target = getAssignedPath(member.Target, rootNode);
                return target?.Concat(new [] { member.MemberName });
            }
            else return null;
        }
        public static void ComputePossibleMutations(ExpressionMutationList result, JsNode node)
        {
            foreach (var n in node.DescendantNodesAndSelf(n => !(n is JsFunctionExpression)))
            {
                if (n is JsAssignmentExpression assignment)
                {
                    var path = getAssignedPath(assignment.Left, node);
                    path?.ApplyAction(result.SetMutatedMember);
                    var rightPath = getAssignedPath(assignment.Right, node);
                    if (path != null && rightPath != null)
                        result.GetMember(rightPath).EqualTo.Add(result.GetMember(path));
                }
                else if (n is JsInvocationExpression invocation)
                {
                    foreach (var arg in invocation.Arguments)
                        getAssignedPath(arg, node)?.ApplyAction(result.SetMutatedMember);
                }
            }
        }

        public static bool AffectsNode(this ExpressionMutationList mutations, JsNode node)
        {
            foreach (var c in node.DescendantNodesAndSelf())
            {
                var path = getAssignedPath(c as JsExpression, node);
                if (path != null && mutations.MayMutate(path))
                    return true;
            }
            return false;
        }

        public static bool ContainsInvocation(JsNode node) => node.DescendantNodesAndSelf().Any(n => n is JsInvocationExpression);

        /// Checks if the specified <see cref="JsNode" /> has any dependencies on the specified mutated variables and if so if it can be reordered.
        public static bool MayReorder(JsNode expression, IEnumerable<JsNode> beforeExpressions)
        {
            // if both contain invocation, reorder can't happen
            if (ContainsInvocation(expression) && beforeExpressions.Any(ContainsInvocation)) return false;

            var effects = GetPossibleMutations(expression);
            return !beforeExpressions.Any(effects.AffectsNode);
        }
    }

    public sealed class ExpressionMutationList
    {
        public Dictionary<object, ExpressionMutationList> Members { get; } = new Dictionary<object, ExpressionMutationList>();
        public List<ExpressionMutationList> EqualTo { get; } = new List<ExpressionMutationList>();
        public bool MutatesRoot { get; set; }
        public bool MayMutate(IEnumerable<object> path) => MayMutate(path.ToImmutableList());
        public bool MayMutate(ImmutableList<object> path, bool allowRoot = true)
        {
            if (MutatesRoot & allowRoot) return true;
            return
                (path.Count > 0 && Members.TryGetValue(path[0], out var next) && next.MayMutate(path.RemoveAt(0))) ||
                EqualTo.Any(e => e.MayMutate(path, allowRoot: false));
        }

        public ExpressionMutationList GetMember(object member)
        {
            if (!this.Members.TryGetValue(member, out var result))
                this.Members.Add(member, result = new ExpressionMutationList());
            return result;
        }

        public ExpressionMutationList GetMember(IEnumerable<object> path) =>
            path?.Aggregate(this, (p, m) => p.GetMember(m));

        public void SetMutatedMember(IEnumerable<object> path) =>
            GetMember(path).MutatesRoot = true;
    }
}
