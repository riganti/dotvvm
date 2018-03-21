using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Symbols.Filters
{
    public interface ISymbolFilter
    {
        bool Matches(ISymbol symbol);
    }
}
