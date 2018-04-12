using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.ViewModel;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
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
        private readonly ISyntaxFactory _factory;

        public TypeSymbolTranslator(ILogger logger, TranslatorsEvidence translatorsEvidence, ISyntaxFactory factory)
        {
            _logger = logger;
            _translatorsEvidence = translatorsEvidence;
            _factory = factory;
        }

        public bool CanTranslate(INamedTypeSymbol input)
        {
            throw new NotImplementedException();
        }

        public ISyntaxNode Translate(INamedTypeSymbol input)
        {
            _logger.LogInfo("Symbols", $"Translating class {input.Name}");
            var members = TranslateProperties(input);
            var methods = TranslateMethods(input);
            members = members.Concat(methods);
            var identifier = TranslateIdentifier(input);
            return _factory.CreateClassDeclaration(identifier, members.ToList(), new List<IIdentifierSyntax>(), null);
        }

        private IEnumerable<IMemberDeclarationSyntax> TranslateMethods(INamedTypeSymbol input)
        {
            return input.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.HasAttribute<ClientSideMethodAttribute>())
                .Where(p => _translatorsEvidence.ResolveTranslator(p).CanTranslate(p))
                .Select(p => _translatorsEvidence.ResolveTranslator(p).Translate(p))
                .OfType<IMemberDeclarationSyntax>();
        }

        private IEnumerable<IMemberDeclarationSyntax> TranslateProperties(INamedTypeSymbol input)
        {
            var propertySymbols = input.GetMembers().OfType<IPropertySymbol>();
            var members = propertySymbols
                .Where(p => _translatorsEvidence.ResolveTranslator(p).CanTranslate(p))
                .Select(p => _translatorsEvidence.ResolveTranslator(p).Translate(p))
                .OfType<IMemberDeclarationSyntax>();
            return members;
        }

        private IIdentifierSyntax TranslateIdentifier(INamedTypeSymbol input)
        {
            return new TsIdentifierSyntax(input.Name, null);
        }
    }
}
