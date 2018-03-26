using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsPropertyDeclarationSyntax : TsMemberDeclarationSyntax
    {
        public TsTypeSyntax Type { get; }

        public TsPropertyDeclarationSyntax(TsModifier modifier, TsIdentifierSyntax identifier, TsTypeSyntax type, TsSyntaxNode parent) : base(modifier, identifier, parent)
        {
            Type = type;
        }

        public override string ToDisplayString()
        {
            return $"{Modifier.ToDisplayString()} {Identifier.ToDisplayString()}: {Type.ToDisplayString()};";
        }
        
        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override TsModifier Modifier { get; protected set; }
        public override TsIdentifierSyntax Identifier { get; set; }
    }
}
