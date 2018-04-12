using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsLocalVariableReferenceSyntax : TsReferenceSyntax, ILocalVariableReferenceSyntax
    {
        public TsLocalVariableReferenceSyntax(ISyntaxNode argument, IIdentifierSyntax identifier) : base(argument)
        {
            Identifier = identifier;
        }

        public override string ToDisplayString()
        {
            return Identifier.ToDisplayString();
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitIdentifierReference(this);
        }
    }
}
