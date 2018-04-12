using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsDoWhileStatementSyntax : TsStatementSyntax, IDoWhileStatementSyntax
    {
        public IExpressionSyntax Condition { get; }
        public IStatementSyntax Body { get; }


        public TsDoWhileStatementSyntax(ISyntaxNode parent, IExpressionSyntax condition, IStatementSyntax body) : base(parent)
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

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            yield return Body;
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitDoWhileStatement(this);
        }
    }
}
