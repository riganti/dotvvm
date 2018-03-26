using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public abstract class TsExpressionSyntax : TsSyntaxNode
    {
        protected TsExpressionSyntax(TsSyntaxNode parent) : base(parent)
        {
        }
    }

    public enum TsBinaryOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Remainder,
        And,
        Or,
        ExclusiveOr,
        ConditionalAnd,
        ConditionalOr,
        Equals,
        ObjectValueEquals,
        NotEquals,
        ObjectValueNotEquals,
        LessThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        GreaterThan,
    }

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
