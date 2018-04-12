using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
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
        private readonly ISyntaxFactory _factory;

        public PropertySymbolTranslator(ILogger logger, ISyntaxFactory factory)
        {
            _logger = logger;
            _factory = factory;
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
            return _factory.CreatePropertyDeclarationSyntax(modifier, identifier, type, null);
        }

        private ITypeSyntax Translatetype(IPropertySymbol property)
        {
            return _factory.CreateType(property.Type, null);
        }

        private IIdentifierSyntax TranslateIdentifier(IPropertySymbol property)
        {
            return _factory.CreateIdentifier(property.Name, null);
        }

        private AccessModifier TranslateModifier(IPropertySymbol property)
        {
            return property.DeclaredAccessibility.ToTsModifier();
        }
    }
}
