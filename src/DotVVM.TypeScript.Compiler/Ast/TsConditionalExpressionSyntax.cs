using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsConditionalExpressionSyntax : TsExpressionSyntax
    {
        public TsExpressionSyntax Condition { get; }
        public TsExpressionSyntax WhenTrue { get; }
        public TsExpressionSyntax WhenFalse { get; }

        public TsConditionalExpressionSyntax(TsSyntaxNode parent, TsExpressionSyntax condition, TsExpressionSyntax whenTrue, TsExpressionSyntax whenFalse) : base(parent)
        {
            Condition = condition;
            WhenTrue = whenTrue;
            WhenFalse = whenFalse;
        }

        public override string ToDisplayString()
        {
            return $"{Condition.ToDisplayString()} ? {WhenTrue.ToDisplayString()} : {WhenFalse.ToDisplayString()}";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(ITsNodeVisitor visitor)
        {
            visitor.VisitConditionalExpression(this);
        }
    }
}
