using System.Linq;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Symbols.Filters
{
    public class ClientSideMethodContainingClassFilter : ISymbolFilter
    {
        private readonly ClientSideMethodFilter innerFilter = new ClientSideMethodFilter();

        public bool Matches(ISymbol symbol)
        {
            return symbol is INamedTypeSymbol named
                   && named.GetMembers().Any(m => innerFilter.Matches(m));
        }
    }
}
