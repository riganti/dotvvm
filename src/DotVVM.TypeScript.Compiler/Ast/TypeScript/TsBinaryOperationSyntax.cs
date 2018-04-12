using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsBinaryOperationSyntax : TsExpressionSyntax, IBinaryOperationSyntax
    {
        public IExpressionSyntax LeftExpression { get; }
        public BinaryOperator Operator { get;  }
        public IExpressionSyntax RightExpression { get; }

        public TsBinaryOperationSyntax(ISyntaxNode parent, IExpressionSyntax leftExpression, IExpressionSyntax rightExpression, BinaryOperator @operator) : base(parent)
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

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitBinaryOperation(this);
        }
    }
}
