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
            var modifier = TranslateModifier(property);
            var identifier = TranslateIdentifier(property);
            var type = Translatetype(property);
            return new TsPropertyDeclarationSyntax(modifier, identifier, type, null);
        }

        private TsTypeSyntax Translatetype(IPropertySymbol property)
        {
            return new TsTypeSyntax(property.Type, null);
        }

        private TsIdentifierSyntax TranslateIdentifier(IPropertySymbol property)
        {
            return new TsIdentifierSyntax(property.Name, null);
        }

        private TsModifier TranslateModifier(IPropertySymbol property)
        {
            return property.DeclaredAccessibility.ToTsModifier();
        }
    }
}
