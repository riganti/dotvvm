using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsUnaryOperationSyntax : TsExpressionSyntax, IUnaryOperationSyntax
    {
        public IExpressionSyntax Operand { get; }
        public UnaryOperator Operator { get;  }

        public TsUnaryOperationSyntax(ISyntaxNode parent, IExpressionSyntax operand, UnaryOperator @operator) : base(parent)
        {
            Operand = operand;
            Operator = @operator;
        }

        public override string ToDisplayString()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            throw new System.NotImplementedException();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitUnaryOperation(this);
        }
    }
}
