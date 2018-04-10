using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsAssignmentSyntax : TsStatementSyntax
    {
        public TsExpressionSyntax Reference { get;  }
        public TsExpressionSyntax Expression { get; }
        
        public TsAssignmentSyntax(TsSyntaxNode parent, TsExpressionSyntax reference, TsExpressionSyntax expression) : base(parent)
        {
            Reference = reference;
            Expression = expression;
        }

        public override string ToDisplayString()
        {
            return $"{Reference.ToDisplayString()} = {Expression.ToDisplayString()}";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(ITsNodeVisitor visitor)
        {
            visitor.VisitAssignmentStatement(this);
        }
    }
}
