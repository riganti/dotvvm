using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public abstract class TsExpressionSyntax : TsSyntaxNode
    {
        protected TsExpressionSyntax(TsSyntaxNode parent) : base(parent)
        {
        }
    }

    public enum TsUnaryOperator
    {
        BitwiseNegation,
        Not,
        Plus,
        Minus,
        True,
        False
    }

    public class TsUnaryOperationSyntax : TsExpressionSyntax
    {
        TsExpressionSyntax Operand { get; }
        TsUnaryOperator Opeator { get;  }

        public TsUnaryOperationSyntax(TsSyntaxNode parent, TsExpressionSyntax operand, TsUnaryOperator opeator) : base(parent)
        {
            Operand = operand;
            Opeator = opeator;
        }

        public override string ToDisplayString()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            throw new System.NotImplementedException();
        }
    }
}
