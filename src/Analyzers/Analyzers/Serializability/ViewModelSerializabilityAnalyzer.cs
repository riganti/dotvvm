﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using DotVVM.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotVVM.Analyzers.Serializability
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
        private const string dotvvmBindAttributeMetadataName = "DotVVM.Framework.ViewModel.BindAttribute";

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
            context.RegisterSemanticModelAction(AnalyzeSyntaxTree);
        }

        private static void AnalyzeSyntaxTree(SemanticModelAnalysisContext context)
        {
            var semanticModel = context.SemanticModel;
            var syntaxTree = context.SemanticModel.SyntaxTree;
            var viewModelInterface = semanticModel.Compilation.GetTypeByMetadataName(dotvvmViewModelInterfaceMetadataName);
            var bindAttribute = semanticModel.Compilation.GetTypeByMetadataName(dotvvmBindAttributeMetadataName);

            // Check all classes
            foreach (var classDeclaration in syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                // Filter out non-ViewModels
                var classInfo = semanticModel.GetDeclaredSymbol(classDeclaration);
                if (!classInfo.AllInterfaces.Any(symbol => symbol.OriginalDefinition.Equals(viewModelInterface)))
                    continue;

                AnalyzeViewModelProperties(classDeclaration, bindAttribute, context);
                AnalyzeViewModelFields(classDeclaration, context);
            }
        }

        /// <summary>
        /// Notify user about unserializable properties and unsupported properties
        /// </summary>
        /// <param name="viewModel">ViewModel class declaration</param>
        /// <param name="context">Semantic context</param>
        private static void AnalyzeViewModelProperties(ClassDeclarationSyntax viewModel, INamedTypeSymbol bindAttribute, SemanticModelAnalysisContext context)
        {
            var semanticModel = context.SemanticModel;
            foreach (var property in viewModel.DescendantNodes().OfType<PropertyDeclarationSyntax>()
                .Where(f => f.Modifiers.Any(m => m.Kind() == SyntaxKind.PublicKeyword)))
            {
                if (semanticModel.GetDeclaredSymbol(property) is not IPropertySymbol propertySymbol)
                    continue;
                if (semanticModel.GetSymbolInfo(property.Type).Symbol is not ITypeSymbol propertyTypeSymbol)
                    continue;

                if (IsSerializationIgnored(propertySymbol, bindAttribute))
                    continue;

                if (property.Type.Kind() == SyntaxKind.NullableType)
                {
                    // Serialization of nullable type
                    var nullable = property.Type as NullableTypeSyntax;
                    if (semanticModel.GetSymbolInfo(nullable!.ElementType).Symbol is not ITypeSymbol elementInfo)
                        continue;

                    if (elementInfo.IsPrimitive())
                        continue;

                    // Serialization of nullables is only supported for primitive types
                    var diagnostic = Diagnostic.Create(UseSerializablePropertiesRule, property.GetLocation(), propertyTypeSymbol.ToDisplayString());
                    context.ReportDiagnostic(diagnostic);
                }
                if (propertyTypeSymbol.IsAbstract)
                {
                    // Serialization of abstract classes can fail
                    var diagnostic = Diagnostic.Create(DoNotUseUninstantiablePropertiesRule, property.GetLocation(), propertyTypeSymbol.ToDisplayString());
                    context.ReportDiagnostic(diagnostic);
                    continue;
                }
                else if (!propertyTypeSymbol.IsSerializationSupported(semanticModel))
                {
                    // Serialization of this specific type is not supported by DotVVM
                    var diagnostic = Diagnostic.Create(UseSerializablePropertiesRule, property.GetLocation(), propertyTypeSymbol.ToDisplayString());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        /// <summary>
        /// Notify user that public fields should not be used to store state of viewModels
        /// </summary>
        /// <param name="viewModel">ViewModel class declaration</param>
        /// <param name="context">Semantic context</param>
        private static void AnalyzeViewModelFields(ClassDeclarationSyntax viewModel, SemanticModelAnalysisContext context)
        {
            foreach (var field in viewModel.DescendantNodes().OfType<FieldDeclarationSyntax>()
                .Where(f => f.Modifiers.Any(m => m.Kind() == SyntaxKind.PublicKeyword)))
            {
                var diagnostic = Diagnostic.Create(DoNotUseFieldsRule, field.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsSerializationIgnored(IPropertySymbol property, INamedTypeSymbol attribute)
        {
            var bindAttribute = property.GetAttributes().SingleOrDefault(a => a.AttributeClass.MetadataName == attribute.MetadataName);
            if (bindAttribute == null || bindAttribute.ConstructorArguments.Length == 0)
                return false;

            if (bindAttribute.ConstructorArguments.First().Value is not int direction)
                return false;

            // Direction.None has value 0 (zero)
            return direction == 0;
        }
    }
}