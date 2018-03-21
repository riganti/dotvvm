using System.Linq;
using System.Text;
using DotVVM.TypeScript.Compiler.Symbols.Filters;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Symbols
{
    internal class SymbolFinder : SymbolVisitor<ISymbol>
    {
        private readonly ISymbolFilter _filter;

        public SymbolFinder(ISymbolFilter filter)
        {
            this._filter = filter;
        }

        public override ISymbol VisitAssembly(IAssemblySymbol symbol)
        {
            return base.VisitAssembly(symbol) ?? symbol.GlobalNamespace.Accept(this);
        }

        public override ISymbol VisitNamespace(INamespaceSymbol symbol)
        {
            return base.VisitNamespace(symbol) ?? symbol.GetMembers().FirstOrDefault(s => s.Accept(this) != null);
        }

        public override ISymbol VisitNamedType(INamedTypeSymbol symbol)
        {
            return base.VisitNamedType(symbol) ?? symbol.GetMembers().FirstOrDefault(s => s.Accept(this) != null);
        }

        public override ISymbol DefaultVisit(ISymbol symbol)
        {
            if (_filter.Matches(symbol)) return symbol;
            return base.DefaultVisit(symbol);
        }
    }
}
