using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsForStatementSyntax : TsStatementSyntax, IForStatementSyntax
    {
        public IStatementSyntax BeforeStatement { get; }
        public IExpressionSyntax Condition { get; }
        public IStatementSyntax AfterExpression { get; }
        public IStatementSyntax Body { get; }

        public TsForStatementSyntax(ISyntaxNode parent, IStatementSyntax beforeStatement, IExpressionSyntax condition, IStatementSyntax afterExpression, IStatementSyntax body) : base(parent)
        {
            BeforeStatement = beforeStatement;
            Condition = condition;
            AfterExpression = afterExpression;
            Body = body;
        }

        public override string ToDisplayString()
        {
            return
                $"for({BeforeStatement.ToDisplayString()}; " +
                $"{Condition.ToDisplayString()}; " +
                $"{AfterExpression.ToDisplayString()})\n" +
                $"{Body.ToDisplayString()}";
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            yield return Body;
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitForStatement(this);
        }
    }
}
