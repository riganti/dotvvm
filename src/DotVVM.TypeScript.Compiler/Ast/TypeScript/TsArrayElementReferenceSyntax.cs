using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    class TsArrayElementReferenceSyntax : TsReferenceSyntax, IArrayElementReferenceSyntax
    {
        public IReferenceSyntax ArrayReference { get; }
        public IExpressionSyntax ItemExpression { get; }

        public TsArrayElementReferenceSyntax(ISyntaxNode parent, IReferenceSyntax arrayReference, IExpressionSyntax itemExpression) : base(parent)
        {
            ArrayReference = arrayReference;
            ItemExpression = itemExpression;
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<ISyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitArrayElementReference(this);
        }
    }
}
