using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsPropertyDeclarationSyntax : TsMemberDeclarationSyntax, IPropertyDeclarationSyntax
    {
        public ITypeSyntax Type { get; }

        public override TsModifier Modifier { get; protected set; }
        public override IIdentifierSyntax Identifier { get; set; }

        public TsPropertyDeclarationSyntax(TsModifier modifier, IIdentifierSyntax identifier, ITypeSyntax type, ISyntaxNode parent) : base(modifier, identifier, parent)
        {
            Type = type;
        }

        public override string ToDisplayString()
        {
            return $"{Modifier.ToDisplayString()} {Identifier.ToDisplayString()}: {Type.ToDisplayString()};";
        }
        
        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitPropertyDeclaration(this);
        }
    }
}
