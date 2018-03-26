using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsAssignmentSyntax : TsStatementSyntax
    {
        public TsIdentifierReferenceSyntax Reference { get;  }
        public TsExpressionSyntax Expression { get; }
        
        public TsAssignmentSyntax(TsSyntaxNode parent, TsIdentifierReferenceSyntax reference, TsExpressionSyntax expression) : base(parent)
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
    }
}
