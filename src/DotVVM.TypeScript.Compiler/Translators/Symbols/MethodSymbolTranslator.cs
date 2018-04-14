using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.Factories;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using DotVVM.TypeScript.Compiler.Symbols;
using DotVVM.TypeScript.Compiler.Translators.Builtin;
using DotVVM.TypeScript.Compiler.Translators.Operations;
using DotVVM.TypeScript.Compiler.Utils;
using DotVVM.TypeScript.Compiler.Utils.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.TypeScript.Compiler.Translators.Symbols
{
    internal class MethodSymbolTranslator : ISymbolTranslator<IMethodSymbol>
    {
        private readonly ILogger _logger;
        private readonly TranslatorsEvidence _translatorsEvidence;
        private readonly CompilerContext _context;
        private readonly ISyntaxFactory _factory;
        private readonly IBuiltinMethodTranslatorRegistry _methodTranslatorRegistry;
        private readonly IBuiltinPropertyTranslatorRegistry _propertyTranslatorRegistry;

        public MethodSymbolTranslator(ILogger logger, TranslatorsEvidence translatorsEvidence,
            CompilerContext context, ISyntaxFactory factory, IBuiltinMethodTranslatorRegistry methodTranslatorRegistry, IBuiltinPropertyTranslatorRegistry propertyTranslatorRegistry)
        {
            _logger = logger;
            _translatorsEvidence = translatorsEvidence;
            _context = context;
            _factory = factory;
            _methodTranslatorRegistry = methodTranslatorRegistry;
            _propertyTranslatorRegistry = propertyTranslatorRegistry;
        }

        public bool CanTranslate(IMethodSymbol input)
        {
            return true;
        }

        public ISyntaxNode Translate(IMethodSymbol input)
        {
            var parameters = TranslateParameters(input);
            var identifier = TranslateIdentifier(input);
            var modifier = TranslateModifier(input);
            var bodyBlock = TranslateBody(input);
            _logger.LogInfo("Symbols", $"Translating method {input.Name}");
            return _factory.CreateMethodDeclaration(modifier, identifier, null, bodyBlock, parameters.ToList());
        }

        private IBlockSyntax TranslateBody(IMethodSymbol input)
        {
            if (input.DeclaringSyntaxReferences.Any())
            {
                var syntaxReference = input.DeclaringSyntaxReferences.First().GetSyntax() as MethodDeclarationSyntax;
                var operation = _context.Compilation.GetSemanticModel(syntaxReference.SyntaxTree).GetOperation(syntaxReference.Body);
                var operationTranslatingVisitor = new OperationTranslatingVisitor(_logger, _factory, _methodTranslatorRegistry, _propertyTranslatorRegistry);
                if (operation.Accept(operationTranslatingVisitor, null) is TsBlockSyntax blockSyntax)
                {
                    return blockSyntax;
                }
            }
            return new TsBlockSyntax(null, new List<IStatementSyntax>());
        }

        private AccessModifier TranslateModifier(IMethodSymbol input)
        {
            return input.DeclaredAccessibility.ToTsModifier();
        }

        
        private IIdentifierSyntax TranslateIdentifier(IMethodSymbol input)
        {
            return _factory.CreateIdentifier(input.Name, null);
        }

        private IEnumerable<IParameterSyntax> TranslateParameters(IMethodSymbol input)
        {
            return input.Parameters.Select(p => _translatorsEvidence.ResolveTranslator(p).Translate(p)).OfType<TsParameterSyntax>();
        }
    }
}
