using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators
{
    public interface ISymbolTranslator<in TInput> : ITranslator<TInput> where TInput : ISymbol 
    {
    }
}
