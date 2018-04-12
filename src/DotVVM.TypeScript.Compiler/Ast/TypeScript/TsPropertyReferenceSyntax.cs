using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsPropertyReferenceSyntax : TsReferenceSyntax, IPropertyReferenceSyntax
    {
        public TsPropertyReferenceSyntax(ISyntaxNode parent, IIdentifierSyntax identifier) : base(parent)
        {
            Identifier = identifier;
        }

        public override string ToDisplayString()
        {
            return $"{Identifier.ToDisplayString()}";
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitPropertyReference(this);
        }
    }
}
