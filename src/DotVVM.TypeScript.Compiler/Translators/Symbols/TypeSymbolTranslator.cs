using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class TypeSymbolTranslator : ISymbolTranslator<ITypeSymbol>
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

        public bool CanTranslate(ITypeSymbol input)
        {
            throw new NotImplementedException();
        }

        public ISyntaxNode Translate(ITypeSymbol input)
        {
            _logger.LogInfo("Symbols", $"Translating class {input.Name}");
            var properties = TranslateProperties(input);
            var methods = TranslateMethods(input);
            var constructors = TranslateConstructors(input);
            var members = properties.Concat(methods).Concat(constructors);
            var identifier = TranslateIdentifier(input);
            return _factory.CreateClassDeclaration(identifier, members.ToList(), new List<IIdentifierSyntax>(), null);
        }

        private IEnumerable<IMemberDeclarationSyntax> TranslateConstructors(ITypeSymbol input)
        {
            if (input is INamedTypeSymbol namedType)
            {
                if (!namedType.Constructors.Any(c => c.Parameters.IsEmpty && !c.IsImplicitlyDeclared))
                {
                    yield return CreateEmptyConstructor(input);
                }
            }
        }

        private IMemberDeclarationSyntax CreateEmptyConstructor(ITypeSymbol input)
        {
            var identifier = _factory.CreateIdentifier("constructor", null);
            var body = _factory.CreateBlock(new List<IStatementSyntax>(), null);
            foreach (var propertySymbol in input.GetMembers().OfType<IPropertySymbol>())
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
                var assignment = _factory.CreateAssignment(propertyReference,expression, null);
                body.AddStatement(assignment);
            }
            return _factory.CreateMethodDeclaration(AccessModifier.None, identifier, null, body, new List<IParameterSyntax>());
        }

        private IEnumerable<IMemberDeclarationSyntax> TranslateMethods(ITypeSymbol input)
        {
            return input.GetBaseTypesIncludingSelfUntil(typeof(DotvvmViewModelBase))
                .SelectMany(t => t.GetMembers())
                .OfType<IMethodSymbol>()
                .Where(m => m.HasAttribute<ClientSideMethodAttribute>())
                .Where(p => _translatorsEvidence.ResolveTranslator(p).CanTranslate(p))
                .Select(p => _translatorsEvidence.ResolveTranslator(p).Translate(p))
                .OfType<IMemberDeclarationSyntax>();
        }

        private IEnumerable<IMemberDeclarationSyntax> TranslateProperties(ITypeSymbol input)
        {
            var propertySymbols = input.GetBaseTypesIncludingSelfUntil(typeof(DotvvmViewModelBase))
                .SelectMany(t => t.GetMembers())
                .OfType<IPropertySymbol>();
            var members = propertySymbols
                .Where(p => _translatorsEvidence.ResolveTranslator(p).CanTranslate(p))
                .Select(p => _translatorsEvidence.ResolveTranslator(p).Translate(p))
                .OfType<IMemberDeclarationSyntax>();
            return members;
        }

        private IIdentifierSyntax TranslateIdentifier(ITypeSymbol input)
        {
            return new TsIdentifierSyntax(input.Name, null);
        }
    }
}
