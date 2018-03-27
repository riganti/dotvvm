using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Symbols
{
    class NamespaceSymbolTranslator : ISymbolTranslator<INamespaceSymbol>
    {
        public bool CanTranslate(INamespaceSymbol input)
        {
            return true;
        }

        public TsSyntaxNode Translate(INamespaceSymbol input)
        {
            var identifier = new TsIdentifierSyntax(input.Name, null);
            var types = new List<TsClassDeclarationSyntax>();
            return new TsNamespaceDeclarationSyntax(null, identifier, types);
        }
    }
}
