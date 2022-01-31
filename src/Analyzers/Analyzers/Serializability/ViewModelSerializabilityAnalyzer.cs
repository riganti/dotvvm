using System;
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
        private const string dotvvmViewModelInterfaceMetadataName =  "DotVVM.Framework.ViewModel.IDotvvmViewModel";
        private const string dotvvmBindAttributeMetadataName = "DotVVM.Framework.ViewModel.BindAttribute";
        private const string newtonsoftJsonIgnoreAttributeMetadataName = "Newtonsoft.Json.JsonIgnoreAttribute";

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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                UseSerializablePropertiesRule,
                DoNotUseFieldsRule);

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
            var jsonIgnoreAttribute = semanticModel.Compilation.GetTypeByMetadataName(newtonsoftJsonIgnoreAttributeMetadataName);
            var serializabilityContext = new SerializabilityAnalysisContext(context, viewModelInterface, bindAttribute, jsonIgnoreAttribute);

            // Check if we found required types
            if (viewModelInterface == null || bindAttribute == null || jsonIgnoreAttribute == null)
                return;

            // Check all classes
            foreach (var classDeclaration in syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                // Filter out non-ViewModels
                var classInfo = semanticModel.GetDeclaredSymbol(classDeclaration);
                if (classInfo == null)
                    continue;

                if (!classInfo.AllInterfaces.Any(symbol => SymbolEqualityComparer.Default.Equals(symbol, viewModelInterface)))
                    continue;

                // TODO: not sure if this is necessary anymore, but keeping it here to not report many false-positives
                if (!SanityCheck(serializabilityContext))
                    return;
                
                AnalyzeViewModelProperties(classDeclaration, serializabilityContext);
                AnalyzeViewModelFields(classDeclaration, context);
            }
        }

        private static bool SanityCheck(SerializabilityAnalysisContext context)
        {
            var testType = context.SemanticModel.Compilation.GetSpecialType(SpecialType.System_String);
            return testType.IsKnownSerializableType(context.SemanticModel.Compilation);
        }

        /// <summary>
        /// Notify user about unserializable properties and unsupported properties
        /// </summary>
        /// <param name="viewModel">ViewModel class declaration</param>
        /// <param name="context">Semantic context</param>
        private static void AnalyzeViewModelProperties(ClassDeclarationSyntax viewModel, SerializabilityAnalysisContext context)
        {
            var semanticModel = context.SemanticModel;
            foreach (var property in viewModel.ChildNodes().OfType<PropertyDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(property) is not IPropertySymbol propertySymbol)
                    continue;

                AnalyzeProperty(propertySymbol, property.GetLocation(), "this", context);
            }
        }

        private static void AnalyzeProperty(IPropertySymbol propertySymbol, Location location, string path, SerializabilityAnalysisContext context)
        {
            // Static and non-public properties do not participate in serialization
            if (propertySymbol.IsStatic || propertySymbol.DeclaredAccessibility != Accessibility.Public)
                return;

            // Serialization might be ignored using attributes
            if (IsSerializationIgnored(propertySymbol, context))
                return;

            AnalyzePropertyType(propertySymbol.Type, location, $"{path}.{propertySymbol.Name}", context);
        }

        private static void AnalyzePropertyType(ITypeSymbol propertyType, Location location, string path, SerializabilityAnalysisContext context)
        {
            // Check if we previously resolved this type
            if (context.IsVisited(propertyType))
            {
                ReportIfNotSerializable(propertyType, location, path, context);
                return;
            }
            context.MarkAsVisited(propertyType);

            // Unwrap type if it is nullable
            if (propertyType.NullableAnnotation == NullableAnnotation.Annotated)
            {
                if (propertyType is not INamedTypeSymbol namedTypeSymbol)
                    return;

                if (namedTypeSymbol.TypeArguments.Any())
                {
                    // Nullable value type
                    propertyType = namedTypeSymbol.TypeArguments.First();
                }
                else
                {
                    // Nullable reference type
                    propertyType = namedTypeSymbol.ConstructedFrom;
                }
            }

            // Issue warning if type is abstract (but omit enumerables - we can serialize these in 99% of cases)
            if (propertyType.IsAbstract && !propertyType.IsEnumerable(context.SemanticModel.Compilation))
            {
                context.ReportDiagnostic(Diagnostic.Create(UseSerializablePropertiesRule, location, path));
                context.MarkAsNotSerializable(propertyType, UseSerializablePropertiesRule);
                return;
            }

            // Otherwise we need to analyze the type further
            switch (propertyType.TypeKind)
            {
                case TypeKind.Array:
                    // We need to ensure array element type is serializable
                    var elementTypeSymbol = ((IArrayTypeSymbol)propertyType).ElementType;
                    if (!context.IsVisited(elementTypeSymbol))
                    {
                        AnalyzePropertyType(elementTypeSymbol, location, path, context);
                    }
                    else
                    {
                        ReportIfNotSerializable(elementTypeSymbol, location, path, context);
                    }
                    break;

                case TypeKind.Enum:
                    // We need to ensure underlying enum type is serializable
                    var underlyingTypeSymbol = ((INamedTypeSymbol)propertyType).EnumUnderlyingType;
                    if (underlyingTypeSymbol != null)
                    {
                        if (!context.IsVisited(underlyingTypeSymbol))
                        {
                            AnalyzePropertyType(underlyingTypeSymbol, location, path, context);
                        }
                        else
                        {
                            ReportIfNotSerializable(underlyingTypeSymbol, location, path, context);
                        }
                    }
                    break;

                case TypeKind.Interface:
                case TypeKind.Class:
                case TypeKind.Struct:
                    var namedTypeSymbol = propertyType as INamedTypeSymbol;
                    var arity = namedTypeSymbol?.Arity;
                    var args = namedTypeSymbol?.TypeArguments ?? ImmutableArray.Create<ITypeSymbol>();
                    var originalDefinitionSymbol = (arity.HasValue && arity.Value > 0) ? propertyType.OriginalDefinition : propertyType;

                    if (originalDefinitionSymbol.IsKnownSerializableType(context.SemanticModel.Compilation) || originalDefinitionSymbol.IsEnumerable(context.SemanticModel.Compilation))
                    {
                        // Type is either primitive and/or directly supported by DotVVM
                        foreach (var arg in args)
                        {
                            if (!context.IsVisited(arg))
                            {
                                AnalyzePropertyType(arg, location, path, context);
                            }
                            else
                            {
                                ReportIfNotSerializable(arg, location, path, context);
                            }
                        }
                    }
                    else if (!propertyType.ContainingNamespace.ToDisplayString().StartsWith("System"))
                    {
                        // User types are supported if all their properties are supported
                        foreach (var property in propertyType.GetMembers().Where(m => m.Kind == SymbolKind.Property).Cast<IPropertySymbol>())
                            AnalyzeProperty(property, location, path, context);
                    }
                    else
                    {
                        // Something unsupported from BCL detected
                        context.ReportDiagnostic(Diagnostic.Create(UseSerializablePropertiesRule, location, path));
                        context.MarkAsNotSerializable(propertyType, UseSerializablePropertiesRule);
                    }
                    break;

                default:
                    context.ReportDiagnostic(Diagnostic.Create(UseSerializablePropertiesRule, location, path));
                    context.MarkAsNotSerializable(propertyType, UseSerializablePropertiesRule);
                    break;
            }
        }

        private static void ReportIfNotSerializable(ITypeSymbol symbol, Location location, string path, SerializabilityAnalysisContext context)
        {
            if (context.IsVisited(symbol))
            {
                var descriptor = context.GetSerializabilityInfo(symbol);
                if (descriptor != null)
                    context.ReportDiagnostic(Diagnostic.Create(descriptor, location, path));
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

        private static bool IsSerializationIgnored(IPropertySymbol property, SerializabilityAnalysisContext context)
        {
            return IsSerializationIgnoredUsingBindAttribute(property, context) || IsSerializationIgnoredUsingJsonIgnoreAttribute(property, context);
        }

        private static bool IsSerializationIgnoredUsingBindAttribute(IPropertySymbol property, SerializabilityAnalysisContext context)
        {
            if (context.BindAttributeSymbol == null)
                return false;

            // Try find attached attribute
            var attribute = property.GetAttributes().SingleOrDefault(a => a.AttributeClass != null && a.AttributeClass.MetadataName == context.BindAttributeSymbol.MetadataName);
            if (attribute == null || attribute.ConstructorArguments.Length == 0)
                return false;

            // Get value provided to ctor
            if (!attribute.ConstructorArguments.Any() || attribute.ConstructorArguments.First().Value is not int direction)
                return false;

            // Direction.None has value 0 (zero)
            return direction == 0;
        }

        private static bool IsSerializationIgnoredUsingJsonIgnoreAttribute(IPropertySymbol property, SerializabilityAnalysisContext context)
        {
            if (context.JsonIgnoreAttributeSymbol == null)
                return false;

            // Try find attached attribute
            if (property.GetAttributes().SingleOrDefault(a => a.AttributeClass != null && a.AttributeClass.MetadataName == context.JsonIgnoreAttributeSymbol.MetadataName) == null)
                return false;

            return true;
        }
    }
}
