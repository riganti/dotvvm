using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsConditionalExpressionSyntax : TsExpressionSyntax, IConditionalExpressionSyntax
    {
        public IExpressionSyntax Condition { get; }
        public IExpressionSyntax WhenTrue { get; }
        public IExpressionSyntax WhenFalse { get; }

        public TsConditionalExpressionSyntax(ISyntaxNode parent, IExpressionSyntax condition, IExpressionSyntax whenTrue, IExpressionSyntax whenFalse) : base(parent)
        {
            Condition = condition;
            WhenTrue = whenTrue;
            WhenFalse = whenFalse;
        }

        public override string ToDisplayString()
        {
            return $"{Condition.ToDisplayString()} ? {WhenTrue.ToDisplayString()} : {WhenFalse.ToDisplayString()}";
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitConditionalExpression(this);
        }
    }
}
