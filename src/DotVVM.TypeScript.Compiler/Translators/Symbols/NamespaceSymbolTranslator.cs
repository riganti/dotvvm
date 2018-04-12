﻿using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Symbols
{
    class NamespaceSymbolTranslator : ISymbolTranslator<INamespaceSymbol>
    {
        public bool CanTranslate(INamespaceSymbol input)
        {
            return true;
        }

        public ISyntaxNode Translate(INamespaceSymbol input)
        {
            var identifier = new TsIdentifierSyntax(input.ToDisplayString(), null);
            var types = new List<IClassDeclarationSyntax>();
            return new TsNamespaceDeclarationSyntax(null, identifier, types);
        }
    }
}
