using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsVariableDeclaratorSyntax : TsSyntaxNode, IVariableDeclaratorSyntax
    {
        public IExpressionSyntax Expression { get; }
        public IIdentifierSyntax Identifier { get; }

        public TsVariableDeclaratorSyntax(ISyntaxNode parent, IExpressionSyntax expression,
            IIdentifierSyntax identifier) : base(parent)
        {
            Expression = expression;
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
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
