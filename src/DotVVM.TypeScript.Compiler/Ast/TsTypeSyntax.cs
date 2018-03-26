using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsTypeSyntax : TsSyntaxNode
    {
        public ITypeSymbol EquivalentSymbol { get; }

        public TsTypeSyntax(ITypeSymbol equivalentSymbol, TsSyntaxNode parent) : base(parent)
        {
            EquivalentSymbol = equivalentSymbol;
        }

        public override string ToDisplayString()
        {
            return EquivalentSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }
    }
}