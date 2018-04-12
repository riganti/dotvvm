using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using DotVVM.TypeScript.Compiler.Symbols;
using DotVVM.TypeScript.Compiler.Utils;
using DotVVM.TypeScript.Compiler.Utils.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotVVM.TypeScript.Compiler.Translators.Symbols
{
    public class TypeSymbolTranslator : ISymbolTranslator<INamedTypeSymbol>
    {
        private readonly ILogger _logger;
        private readonly TranslatorsEvidence _translatorsEvidence;

        public TypeSymbolTranslator(ILogger logger, TranslatorsEvidence translatorsEvidence)
        {
            _logger = logger;
            _translatorsEvidence = translatorsEvidence;
        }

        public bool CanTranslate(INamedTypeSymbol input)
        {
            throw new NotImplementedException();
        }

        public ISyntaxNode Translate(INamedTypeSymbol input)
        {
            var propertySymbols = input.GetMembers().OfType<IPropertySymbol>();
            var members = propertySymbols
                .Where(p => _translatorsEvidence.ResolveTranslator(p).CanTranslate(p))
                .Select(p => _translatorsEvidence.ResolveTranslator(p).Translate(p))
                .OfType<IMemberDeclarationSyntax>();
            var methods = input.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.HasAttribute<ClientSideMethodAttribute>())
                .Where(p => _translatorsEvidence.ResolveTranslator(p).CanTranslate(p))
                .Select(p => _translatorsEvidence.ResolveTranslator(p).Translate(p))
                .OfType<IMemberDeclarationSyntax>();
            members = members.Concat(methods);
            _logger.LogInfo("Symbols", $"Translating class {input.Name}");
            return new TsClassDeclarationSyntax(null, new TsIdentifierSyntax(input.Name, null), members.ToList(), new List<IIdentifierSyntax>());
        }
    }
}
