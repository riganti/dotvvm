using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsAssignmentSyntax : TsStatementSyntax
    {
        public TsIdentifierSyntax Identifier { get;  }
        public TsExpressionSyntax Expression { get; }
        
        public TsAssignmentSyntax(TsSyntaxNode parent, TsIdentifierSyntax identifier, TsExpressionSyntax expression) : base(parent)
        {
            Identifier = identifier;
            Expression = expression;
        }

        public override string ToDisplayString()
        {
            return $"{Identifier.ToDisplayString()} = {Expression.ToDisplayString()}";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }
    }
}
