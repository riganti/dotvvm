using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsVariableDeclaratorSyntax : TsSyntaxNode
    {
        public TsExpressionSyntax Expression { get; }
        public TsIdentifierSyntax Identifier { get; }
        
        public TsVariableDeclaratorSyntax(TsSyntaxNode parent, TsExpressionSyntax expression, TsIdentifierSyntax identifier) : base(parent)
        {
            Expression = expression;
            Identifier = identifier;
        }

        public override string ToDisplayString()
        {
            if (Expression != null)
            {
                return $"{Identifier.ToDisplayString()} = {Expression.ToDisplayString()}";
            }
            else
            {
                return Identifier.ToDisplayString();
            }
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }
    }
}