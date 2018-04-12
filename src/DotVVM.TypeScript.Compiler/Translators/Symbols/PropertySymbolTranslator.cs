using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using DotVVM.TypeScript.Compiler.Symbols;
using DotVVM.TypeScript.Compiler.Utils;
using DotVVM.TypeScript.Compiler.Utils.Logging;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Symbols
{
    public class PropertySymbolTranslator : ISymbolTranslator<IPropertySymbol>
    {
        private readonly ILogger _logger;

        public PropertySymbolTranslator(ILogger logger)
        {
            _logger = logger;
        }

        public bool CanTranslate(IPropertySymbol input)
        {
            return true;
        }

        public ISyntaxNode Translate(IPropertySymbol property)
        {
            var modifier = TranslateModifier(property);
            var identifier = TranslateIdentifier(property);
            var type = Translatetype(property);
            _logger.LogInfo("Symbols", $"Translating property {property.Name}");
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

        private AccessModifier TranslateModifier(IPropertySymbol property)
        {
            return property.DeclaredAccessibility.ToTsModifier();
        }
    }
}
