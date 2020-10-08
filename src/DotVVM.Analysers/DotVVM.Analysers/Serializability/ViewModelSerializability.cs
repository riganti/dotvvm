using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotVVM.Analysers.Serializability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ViewModelSerializability : DiagnosticAnalyzer
    {
        private static readonly LocalizableResourceString localizableTitle = new LocalizableResourceString(nameof(Resources.ViewModelSerializabilityTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString localizableMessage = new LocalizableResourceString(nameof(Resources.ViewModelSerializabilityMessage), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString localizableDescription = new LocalizableResourceString(nameof(Resources.ViewModelSerializabilityDescription), Resources.ResourceManager, typeof(Resources));

        public static DiagnosticDescriptor UseSerializablePropertiesRule = new DiagnosticDescriptor(
            DotvvmDiagnosticIds.PropertiesSerializabilityRuleId,
            localizableTitle,
            localizableMessage,
            "Serializability",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            localizableDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(UseSerializablePropertiesRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterSemanticModelAction(AnalyzeViewModelProperties);
        }

        private static void AnalyzeViewModelProperties(SemanticModelAnalysisContext context)
        {
            var semanticModel = context.SemanticModel;
            var syntaxTree = context.SemanticModel.SyntaxTree;

            // Check all classes
            foreach (var classDeclaration in syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                // Filter out non-ViewModels
                var classInfo = semanticModel.GetDeclaredSymbol(classDeclaration);
                if (!classInfo.AllInterfaces.Any(symbol => symbol.Name == "IDotvvmViewModel"))
                    continue;

                // Check all properties
                foreach (var property in classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>())
                {
                    // Check that symbol is available
                    var propertyInfo = semanticModel.GetSymbolInfo(property.Type).Symbol as INamedTypeSymbol;
                    if (propertyInfo == null)
                        continue;

                    // Filter out serializable properties
                    if (propertyInfo.IsSerializable())
                        continue;

                    // For all such symbols, produce a diagnostic.
                    var diagnostic = Diagnostic.Create(UseSerializablePropertiesRule, property.GetLocation(), propertyInfo.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
