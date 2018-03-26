using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Symbols;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Symbols
{
    public class PropertySymbolTranslator : ISymbolTranslator<IPropertySymbol>
    {
        public bool CanTranslate(IPropertySymbol input)
        {
            return true;
        }

        

        public TsSyntaxNode Translate(IPropertySymbol property)
        {
            return new TsPropertyDeclarationSyntax(property.DeclaredAccessibility.ToTsModifier(), new TsIdentifierSyntax(property.Name, null), new TsTypeSyntax(property.Type, null), null);
        }
    }
}
