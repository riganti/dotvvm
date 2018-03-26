using DotVVM.TypeScript.Compiler.Ast;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Symbols
{
    internal class ParameterSymbolTranslator : ISymbolTranslator<IParameterSymbol>
    {
        public bool CanTranslate(IParameterSymbol input)
        {
            return true;
        }

        public TsSyntaxNode Translate(IParameterSymbol input)
        {
            return new TsParameterSyntax(null, new TsTypeSyntax(input.Type, null), new TsIdentifierSyntax(input.Name, null) );
        }
    }
}
