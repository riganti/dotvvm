using System;
using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsIfStatementSyntax : TsStatementSyntax, IIfStatementSyntax
    {
        public IExpressionSyntax ConditionalExpression { get; }
        public IStatementSyntax TrueStatement { get; }
        public IStatementSyntax FalseStatement { get;  }


        public TsIfStatementSyntax(ISyntaxNode parent, IExpressionSyntax conditionalExpression,
            IStatementSyntax trueStatement, IStatementSyntax falseStatement) : base(parent)
        {
            ConditionalExpression =
                conditionalExpression ?? throw new ArgumentNullException(nameof(conditionalExpression));
            TrueStatement = trueStatement ?? throw new ArgumentNullException(nameof(trueStatement));
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

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            throw new System.NotImplementedException();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitIfStatement(this);
        }
    }
}
