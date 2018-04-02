using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsWhileStatementSyntax : TsStatementSyntax
    {
        public TsExpressionSyntax Condition { get; }
        public TsStatementSyntax Body { get; }

        public TsWhileStatementSyntax(TsSyntaxNode parent, TsExpressionSyntax condition, TsStatementSyntax body) : base(parent)
        {
            Condition = condition;
            Body = body;
        }

        public override string ToDisplayString()
        {
            return $"while({Condition.ToDisplayString()})\n" +
                   $"\t{Body.ToDisplayString()}";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            yield return Body;
        }

        public override void AcceptVisitor(ITsNodeVisitor visitor)
        {
            visitor.VisitWhileStatement(this);
        }
    }
}
