using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using DotVVM.TypeScript.Compiler.Symbols;
using DotVVM.TypeScript.Compiler.Symbols.Registries;
using DotVVM.TypeScript.Compiler.Utils;
using DotVVM.TypeScript.Compiler.Utils.Logging;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Symbols
{
    public class PropertySymbolTranslator : ISymbolTranslator<IPropertySymbol>
    {
        private readonly ILogger _logger;
        private readonly ISyntaxFactory _factory;
        private readonly TypeRegistry _typeRegistry;

        public PropertySymbolTranslator(ILogger logger, ISyntaxFactory factory, TypeRegistry typeRegistry)
        {
            _logger = logger;
            _factory = factory;
            _typeRegistry = typeRegistry;
        }

        public bool CanTranslate(IPropertySymbol input)
        {
            return true;
        }

        public ISyntaxNode Translate(IPropertySymbol property)
        {
            _logger.LogInfo("Symbols", $"Translating property {property.Name}");
            var modifier = TranslateModifier(property);
            var identifier = TranslateIdentifier(property);
            var type = Translatetype(property);
            _typeRegistry.RegisterType(property.Type as INamedTypeSymbol);
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
