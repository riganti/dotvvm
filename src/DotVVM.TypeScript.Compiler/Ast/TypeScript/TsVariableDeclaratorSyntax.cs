using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsVariableDeclaratorSyntax : TsSyntaxNode, IVariableDeclaratorSyntax
    {
        public IExpressionSyntax Expression { get; }
        public IIdentifierSyntax Identifier { get; }
        
        public TsVariableDeclaratorSyntax(ISyntaxNode parent, IExpressionSyntax expression, IIdentifierSyntax identifier) : base(parent)
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

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitVariableDeclarator(this);
        }
    }
}
