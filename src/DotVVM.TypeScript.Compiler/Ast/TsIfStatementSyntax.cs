using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsIfStatementSyntax : TsStatementSyntax
    {
        public TsExpressionSyntax ConditionalExpression { get; }
        public TsStatementSyntax TrueStatement { get; }
        public TsStatementSyntax FalseStatement { get;  }


        public TsIfStatementSyntax(TsSyntaxNode parent, TsExpressionSyntax conditionalExpression, TsStatementSyntax trueStatement, TsStatementSyntax falseStatement) : base(parent)
        {
            ConditionalExpression = conditionalExpression;
            TrueStatement = trueStatement;
            FalseStatement = falseStatement;
        }

        public override string ToDisplayString()
        {
            var output = $"if({ConditionalExpression.ToDisplayString()})\n";
            output += $"{TrueStatement.ToDisplayString()}";
            if (FalseStatement != null)
            {
                output += "else\n";
                output += $"{FalseStatement.ToDisplayString()}";
            }
            return output;
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            throw new System.NotImplementedException();
        }

        public override void AcceptVisitor(ITsNodeVisitor visitor)
        {
            visitor.VisitIfStatement(this);
        }
    }
}
