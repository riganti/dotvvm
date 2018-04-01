using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsForStatementSyntax : TsStatementSyntax
    {
        public TsStatementSyntax BeforeStatement { get; }
        public TsExpressionSyntax Condition { get; }
        public TsStatementSyntax AfterExpression { get; }
        public TsStatementSyntax Body { get; }

        public TsForStatementSyntax(TsSyntaxNode parent, TsStatementSyntax beforeStatement, TsExpressionSyntax condition, TsStatementSyntax afterExpression, TsStatementSyntax body) : base(parent)
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

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            yield return Body;
        }
    }
}