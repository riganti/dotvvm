using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Symbols;
using DotVVM.TypeScript.Compiler.Translators.Operations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.TypeScript.Compiler.Translators.Symbols
{
    internal class MethodSymbolTranslator : ISymbolTranslator<IMethodSymbol>
    {
        private readonly TranslatorsEvidence _translatorsEvidence;
        private readonly CompilerContext _context;

        public MethodSymbolTranslator(TranslatorsEvidence translatorsEvidence, CompilerContext context)
        {
            _translatorsEvidence = translatorsEvidence;
            _context = context;
        }

        public bool CanTranslate(IMethodSymbol input)
        {
            return true;
        }

        public TsSyntaxNode Translate(IMethodSymbol input)
        {
            var parameters = TranslateParameters(input);
            var identifier = TranslateIdentifier(input);
            var modifier = TranslateModifier(input);
            var bodyBlock = TranslateBody(input);
            return new TsMethodDeclarationSyntax(modifier,
                identifier,
                null,
                bodyBlock,
                parameters.ToList(),
                null);
        }

        private TsBlockSyntax TranslateBody(IMethodSymbol input)
        {
            if (input.DeclaringSyntaxReferences.Any())
            {
                var syntaxReference = input.DeclaringSyntaxReferences.First().GetSyntax() as MethodDeclarationSyntax;
                var operation = _context.Compilation.GetSemanticModel(syntaxReference.SyntaxTree).GetOperation(syntaxReference.Body);
                var operationTranslatingVisitor = new OperationTranslatingVisitor();
                if (operation.Accept(operationTranslatingVisitor, null) is TsBlockSyntax blockSyntax)
                {
                    return blockSyntax;
                }
            }
            return new TsBlockSyntax(null, new List<TsStatementSyntax>());
        }

        private TsModifier TranslateModifier(IMethodSymbol input)
        {
            return input.DeclaredAccessibility.ToTsModifier();
        }

        private TsIdentifierSyntax TranslateIdentifier(IMethodSymbol input)
        {
            return new TsIdentifierSyntax(input.Name, null);
        }

        private IEnumerable<TsParameterSyntax> TranslateParameters(IMethodSymbol input)
        {
            return input.Parameters.Select(p => _translatorsEvidence.ResolveTranslator(p).Translate(p)).OfType<TsParameterSyntax>();
        }
    }
}
