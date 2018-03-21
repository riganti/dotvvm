using DotVVM.Framework.ViewModel;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Symbols.Filters
{
    public class ClientSideMethodFilter : ISymbolFilter
    {
        public bool Matches(ISymbol symbol)
        {
            return symbol is IMethodSymbol
                && symbol.HasAttribute<ClientSideMethodAttribute>();
        }
    }
}
