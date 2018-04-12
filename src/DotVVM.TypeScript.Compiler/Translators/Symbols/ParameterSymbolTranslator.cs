using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using DotVVM.TypeScript.Compiler.Utils;
using DotVVM.TypeScript.Compiler.Utils.Logging;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Symbols
{
    internal class ParameterSymbolTranslator : ISymbolTranslator<IParameterSymbol>
    {
        private readonly ILogger _logger;

        public ParameterSymbolTranslator(ILogger logger)
        {
            _logger = logger;
        }

        public bool CanTranslate(IParameterSymbol input)
        {
            return true;
        }

        public ISyntaxNode Translate(IParameterSymbol input)
        {
            _logger.LogInfo("Symbols", $"Translating method parameter {input.Name}");
            return new TsParameterSyntax(null, new TsTypeSyntax(input.Type, null), new TsIdentifierSyntax(input.Name, null) );
        }
    }
}
