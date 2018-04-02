using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsDoWhileStatementSyntax : TsStatementSyntax
    {
        public TsExpressionSyntax Condition { get; }
        public TsStatementSyntax Body { get; }


        public TsDoWhileStatementSyntax(TsSyntaxNode parent, TsExpressionSyntax condition, TsStatementSyntax body) : base(parent)
        {
            Condition = condition;
            Body = body;
        }

        public override string ToDisplayString()
        {
            return $"do \n" +
                   $"\t{Body.ToDisplayString()}\n" +
                   $"while({Condition.ToDisplayString()})";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            yield return Body;
        }

        public override void AcceptVisitor(ITsNodeVisitor visitor)
        {
            visitor.VisitDoWhileStatement(this);
        }
    }
}
