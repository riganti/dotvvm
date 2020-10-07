using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotVVM.Analysers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DotVVMAnalysersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DotVVM";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Serializability";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSemanticModelAction(AnalyzeViewModelProperties);
        }

        private static bool IsPrimitiveType(ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Byte:
                case SpecialType.System_Char:
                case SpecialType.System_Double:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                case SpecialType.System_SByte:
                case SpecialType.System_Single:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsSerializable(ITypeSymbol type)
        {
            if (IsPrimitiveType(type))
                return true;

            switch (type.TypeKind)
            {
                case TypeKind.Array:
                    return IsSerializable(((IArrayTypeSymbol)type).ElementType);

                case TypeKind.Enum:
                    return IsSerializable(((INamedTypeSymbol)type).EnumUnderlyingType);

                case TypeKind.TypeParameter:
                case TypeKind.Interface:
                    // The concrete type can't be determined statically,
                    // so we assume true to cut down on noise.
                    return true;

                case TypeKind.Class:
                case TypeKind.Struct:
                    // Check SerializableAttribute or Serializable flag from metadata.
                    return ((INamedTypeSymbol)type).IsSerializable;

                case TypeKind.Delegate:
                    // delegates are always serializable, even if
                    // they aren't actually marked [Serializable]
                    return true;

                default:
                    return type.GetAttributes().Any(a => a.AttributeClass.Name == "SerializableAttribute");
            }
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
                    if (IsSerializable(propertyInfo))
                        continue;

                    // For all such symbols, produce a diagnostic.
                    var diagnostic = Diagnostic.Create(Rule, property.GetLocation(), propertyInfo.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
