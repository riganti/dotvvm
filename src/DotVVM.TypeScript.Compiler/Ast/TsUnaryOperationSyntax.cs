using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsUnaryOperationSyntax : TsExpressionSyntax
    {
        public TsExpressionSyntax Operand { get; }
        public TsUnaryOperator Operator { get;  }

        public TsUnaryOperationSyntax(TsSyntaxNode parent, TsExpressionSyntax operand, TsUnaryOperator @operator) : base(parent)
        {
            Operand = operand;
            Operator = @operator;
        }

        public override string ToDisplayString()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            throw new System.NotImplementedException();
        }

        public override void AcceptVisitor(ITsNodeVisitor visitor)
        {
            visitor.VisitUnaryOperation(this);
        }
    }
}
