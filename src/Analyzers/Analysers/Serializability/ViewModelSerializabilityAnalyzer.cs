using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Analysers;
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
        private static readonly LocalizableResourceString nonSerializableTypeDescription = new LocalizableResourceString(nameof(Resources.Serializability_NonSerializableType_Description), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString doNotUseFieldsTitle = new LocalizableResourceString(nameof(Resources.Serializability_DoNotUseFields_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString doNotUseFieldsMessage = new LocalizableResourceString(nameof(Resources.Serializability_DoNotUseFields_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString doNotUseFieldsDescription = new LocalizableResourceString(nameof(Resources.Serializability_DoNotUseFields_Description), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString uninstantiableTypeTitle = new LocalizableResourceString(nameof(Resources.Serializability_UninstantiableType_Title), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString uninstantiableTypeMessage = new LocalizableResourceString(nameof(Resources.Serializability_UninstantiableType_Message), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableResourceString uninstantiableTypeDescription = new LocalizableResourceString(nameof(Resources.Serializability_UninstantiableType_Description), Resources.ResourceManager, typeof(Resources));
        private const string dotvvmViewModelInterfaceMetadataName =  "DotVVM.Framework.ViewModel.IDotvvmViewModel";

        public static DiagnosticDescriptor UseSerializablePropertiesRule = new DiagnosticDescriptor(
            DotvvmDiagnosticIds.UseSerializablePropertiesInViewModelRuleId,
            nonSerializableTypeTitle,
            nonSerializableTypeMessage,
            DiagnosticCategory.Serializability,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            nonSerializableTypeDescription);

        public static DiagnosticDescriptor DoNotUseFieldsRule = new DiagnosticDescriptor(
            DotvvmDiagnosticIds.DoNotUseFieldsInViewModelRuleId,
            doNotUseFieldsTitle,
            doNotUseFieldsMessage,
            DiagnosticCategory.Serializability,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            doNotUseFieldsDescription);

        public static DiagnosticDescriptor DoNotUseUninstantiablePropertiesRule = new DiagnosticDescriptor(
            DotvvmDiagnosticIds.DoNotUseUninstantiablePropertiesInViewModelRuleId,
            uninstantiableTypeTitle,
            uninstantiableTypeMessage,
            DiagnosticCategory.Serializability,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            uninstantiableTypeDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                UseSerializablePropertiesRule,
                DoNotUseFieldsRule,
                DoNotUseUninstantiablePropertiesRule);

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
                    if (semanticModel.GetSymbolInfo(property.Type).Symbol is not ITypeSymbol propertyInfo)
                        continue;

                    if (propertyInfo.IsAbstract)
                    {
                        // Serialization of abstract classes can fail
                        var diagnostic = Diagnostic.Create(DoNotUseUninstantiablePropertiesRule, property.GetLocation(), propertyInfo.ToDisplayString());
                        context.ReportDiagnostic(diagnostic);
                        continue;
                    }
                    else if (!propertyInfo.IsSerializationSupported(semanticModel))
                    {
                        // Serialization of this specific type is not supported by DotVVM
                        var diagnostic = Diagnostic.Create(UseSerializablePropertiesRule, property.GetLocation(), propertyInfo.ToDisplayString());
                        context.ReportDiagnostic(diagnostic);
                    }
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
