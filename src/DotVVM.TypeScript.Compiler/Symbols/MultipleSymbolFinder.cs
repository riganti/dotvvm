using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Symbols.Filters;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Symbols
{
    public class MultipleSymbolFinder : SymbolVisitor<List<ISymbol>>
    {
        private ISymbolFilter filter;
        private List<ISymbol> matches;
        public MultipleSymbolFinder(ISymbolFilter filter)
        {
            this.filter = filter;
            this.matches = new List<ISymbol>();
        }


        public override List<ISymbol> VisitAssembly(IAssemblySymbol symbol)
        {
            base.VisitAssembly(symbol);
            symbol.GlobalNamespace.Accept(this);
            return matches;
        }

        public override List<ISymbol> VisitNamespace(INamespaceSymbol symbol)
        {
            base.VisitNamespace(symbol);
            foreach (var member in symbol.GetMembers())
            {
                member.Accept(this);
            }
            return matches;
        }

        public override List<ISymbol> VisitNamedType(INamedTypeSymbol symbol)
        {
            base.VisitNamedType(symbol);
            foreach (var member in symbol.GetMembers())
            {
                member.Accept(this);
            }
            return matches;
        }

        public override List<ISymbol> DefaultVisit(ISymbol symbol)
        {
            if (filter.Matches(symbol)) matches.Add(symbol);
            return base.DefaultVisit(symbol);
        }


    }
}
