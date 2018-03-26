using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsBinaryOperationSyntax : TsExpressionSyntax
    {
        public TsExpressionSyntax LeftExpression { get; }
        public TsBinaryOperator Operator { get;  }
        public TsExpressionSyntax RightExpression { get; }

        public TsBinaryOperationSyntax(TsSyntaxNode parent, TsExpressionSyntax leftExpression, TsExpressionSyntax rightExpression, TsBinaryOperator @operator) : base(parent)
        {
            LeftExpression = leftExpression;
            RightExpression = rightExpression;
            Operator = @operator;
        }

        public override string ToDisplayString()
        {
            return
                $"{LeftExpression.ToDisplayString()} {Operator.ToDisplayString()} {RightExpression.ToDisplayString()}";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            throw new System.NotImplementedException();
        }
    }
}