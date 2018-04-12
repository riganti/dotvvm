using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using DotVVM.TypeScript.Compiler.Utils;
using DotVVM.TypeScript.Compiler.Utils.Logging;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Symbols
{
    internal class ParameterSymbolTranslator : ISymbolTranslator<IParameterSymbol>
    {
        private readonly ILogger _logger;
        private readonly ISyntaxFactory _factory;

        public ParameterSymbolTranslator(ILogger logger, ISyntaxFactory factory)
        {
            _logger = logger;
            _factory = factory;
        }

        public bool CanTranslate(IParameterSymbol input)
        {
            return true;
        }

        public ISyntaxNode Translate(IParameterSymbol input)
        {
            _logger.LogInfo("Symbols", $"Translating method parameter {input.Name}");
            var identifier = TranslateIdentifier(input);
            var type = TranslateType(input);
            return _factory.CreateParameter(identifier, type, null);
        }

        private IIdentifierSyntax TranslateIdentifier(IParameterSymbol input)
        {
            return _factory.CreateIdentifier(input.Name, null);
        }

        private ITypeSyntax TranslateType(IParameterSymbol input)
        {
            return _factory.CreateType(input.Type, null);
        }
    }
}
