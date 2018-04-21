using System.Collections.Generic;
using System.Collections.Immutable;
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
                var syntaxReference = input.DeclaringSyntaxReferences.First().GetSyntax() as BaseMethodDeclarationSyntax;
                var operation = _context.Compilation.GetSemanticModel(syntaxReference.SyntaxTree).GetOperation(syntaxReference.Body);
                var operationTranslatingVisitor = new OperationTranslatingVisitor(_logger, _factory, _methodTranslatorRegistry, _propertyTranslatorRegistry);
                if (operation.Accept(operationTranslatingVisitor, null) is IBlockSyntax blockSyntax)
                {
                    if (input.MethodKind == MethodKind.Constructor)
                    {
                        return WrapInObservableInititalization(input, blockSyntax);
                    }
                    return blockSyntax;
                }
            }
            return _factory.CreateBlock(new List<IStatementSyntax>(),null);
        }

        private IBlockSyntax WrapInObservableInititalization(IMethodSymbol input, IBlockSyntax blockSyntax)
        {
            var containingType = input.ContainingType;
            var body = _factory.CreateBlock(new List<IStatementSyntax>(), null);
            var propertySymbols = containingType.GetMembers()
                .OfType<IPropertySymbol>();
            foreach (var propertySymbol in propertySymbols)
            {
                var propertyIdentifier = _factory.CreateIdentifier($"this.{propertySymbol.Name}", null);
                var propertyReference = _factory.CreateLocalVariableReference(propertyIdentifier, null);
                IExpressionSyntax expression;
                if (propertySymbol.Type.IsArrayType() && !propertySymbol.Type.IsStringType())
                {
                    var koVariableReference = _factory.CreateLocalVariableReference(_factory.CreateIdentifier("ko", null), null);
                    expression = _factory.CreateMethodCall(koVariableReference, _factory.CreateIdentifier("observableArray", null),
                        ImmutableList<IExpressionSyntax>.Empty, null);
                }
                else
                {
                    var koVariableReference = _factory.CreateLocalVariableReference(_factory.CreateIdentifier("ko", null), null);
                    expression = _factory.CreateMethodCall(koVariableReference, _factory.CreateIdentifier("observable", null),
                        ImmutableList<IExpressionSyntax>.Empty, null);
                }
                var assignment = _factory.CreateAssignment(propertyReference, expression, null);
                body.AddStatement(assignment);
            }
            foreach (var blockSyntaxStatement in blockSyntax.Statements)
            {
                body.AddStatement(blockSyntaxStatement);
            }
            return body;
        }

        private AccessModifier TranslateModifier(IMethodSymbol input)
        {
            return input.DeclaredAccessibility.ToTsModifier();
        }

        
        private IIdentifierSyntax TranslateIdentifier(IMethodSymbol input)
        {
            if (input.MethodKind == MethodKind.Constructor)
                return _factory.CreateIdentifier("constructor", null);
            else 
                return _factory.CreateIdentifier(input.Name, null);
        }

        private IEnumerable<IParameterSyntax> TranslateParameters(IMethodSymbol input)
        {
            return input.Parameters.Select(p => _translatorsEvidence.ResolveTranslator(p).Translate(p)).OfType<IParameterSyntax>();
        }
    }
}
