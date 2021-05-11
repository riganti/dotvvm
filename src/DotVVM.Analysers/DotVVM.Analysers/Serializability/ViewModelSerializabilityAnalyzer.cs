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
    public sealed class ViewModelSerializabilityAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableResourceString nonSerializableTypeTitle = new LocalizableResourceString(nameof(Resources.Serializability_NonSerializableType_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString nonSerializableTypeMessage = new LocalizableResourceString(nameof(Resources.Serializability_NonSerializableType_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString nonSupportedTypeMessage = new LocalizableResourceString(nameof(Resources.Serializability_NonSupportedType_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString nonSerializableTypeDescription = new LocalizableResourceString(nameof(Resources.Serializability_NonSerializableType_Description), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString doNotUseFieldsTitle = new LocalizableResourceString(nameof(Resources.Serializability_DoNotUseFields_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString doNotUseFieldsMessage = new LocalizableResourceString(nameof(Resources.Serializability_DoNotUseFields_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString doNotUseFieldsDescription = new LocalizableResourceString(nameof(Resources.Serializability_DoNotUseFields_Description), Resources.ResourceManager, typeof(Resources));
        private const string dotvvmViewModelInterfaceMetadataName =  "DotVVM.Framework.ViewModel.IDotvvmViewModel";

        public static DiagnosticDescriptor UseSerializablePropertiesRule = new DiagnosticDescriptor(
            DotvvmDiagnosticIds.UseSerializablePropertiesRuleId,
            nonSerializableTypeTitle,
            nonSerializableTypeMessage,
            DiagnosticCategory.Serializability,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            nonSerializableTypeDescription);

        public static DiagnosticDescriptor UseSupportedPropertiesRule = new DiagnosticDescriptor(
            DotvvmDiagnosticIds.UseSerializablePropertiesRuleId,
            nonSerializableTypeTitle,
            nonSupportedTypeMessage,
            DiagnosticCategory.Serializability,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            nonSerializableTypeDescription);

        public static DiagnosticDescriptor DoNotUseFieldsRule = new DiagnosticDescriptor(
            DotvvmDiagnosticIds.DoNotUseFieldsRuleId,
            doNotUseFieldsTitle,
            doNotUseFieldsMessage,
            DiagnosticCategory.Serializability,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            doNotUseFieldsDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                UseSerializablePropertiesRule,
                DoNotUseFieldsRule);

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
            var viewModelInterface = semanticModel.Compilation.GetTypeByMetadataName(dotvvmViewModelInterfaceMetadataName);

            // Check all classes
            foreach (var classDeclaration in syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                // Filter out non-ViewModels
                var classInfo = semanticModel.GetDeclaredSymbol(classDeclaration);
                if (!classInfo.AllInterfaces.Any(symbol => SymbolEqualityComparer.Default.Equals(symbol.OriginalDefinition, viewModelInterface)))
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

                // Check if any fields are specified
                foreach (var field in classDeclaration.DescendantNodes().OfType<FieldDeclarationSyntax>())
                {
                    var diagnostic = Diagnostic.Create(DoNotUseFieldsRule, field.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
