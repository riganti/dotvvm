using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Analyzers.Serializability
{
    internal static class TypeSymbolExtensions
    {
        private static ImmutableHashSet<ISymbol> supportedReferenceTypesForSerialization = ImmutableHashSet.Create<ISymbol>(SymbolEqualityComparer.Default);
        private static ImmutableHashSet<ISymbol> supportedValueTypesForSerilization = ImmutableHashSet.Create<ISymbol>(SymbolEqualityComparer.Default);
        private static volatile bool cachesConstructed = false;

        public static bool IsSerializationSupported(this ITypeSymbol typeSymbol, SemanticModel semantics)
        {
            EnsureCachesAreConstructed(semantics.Compilation);
            return IsSerializationSupportedImpl(typeSymbol);
        }

        public static bool IsReferenceTypeSerializationSupported(this ITypeSymbol referenceTypeSymbol, Compilation compilation)
        {
            EnsureCachesAreConstructed(compilation);
            return supportedReferenceTypesForSerialization.Contains(referenceTypeSymbol);
        }

        public static bool IsValueTypeSerializationSupported(this ITypeSymbol valueTypeSymbol, Compilation compilation)
        {
            EnsureCachesAreConstructed(compilation);
            return supportedValueTypesForSerilization.Contains(valueTypeSymbol);
        }

        private static void EnsureCachesAreConstructed(Compilation compilation)
        {
            if (!cachesConstructed)
                CreateTypesCache(compilation);
        }

        private static bool IsSerializationSupportedImpl(this ITypeSymbol typeSymbol)
        {
            var stack = new Stack<ITypeSymbol>();
#pragma warning disable RS1024 // Compare symbols correctly
            // This is a false positive: https://github.com/dotnet/roslyn-analyzers/issues/4568
            var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default) { typeSymbol };
#pragma warning restore RS1024 // Compare symbols correctly
            stack.Push(typeSymbol);

            while (stack.Count != 0)
            {
                var currentSymbol = stack.Pop();
                switch (currentSymbol.TypeKind)
                {
                    case TypeKind.Array:
                        var elementTypeSymbol = ((IArrayTypeSymbol)currentSymbol).ElementType;
                        if (!visited.Contains(elementTypeSymbol))
                        {
                            stack.Push(elementTypeSymbol);
                            visited.Add(elementTypeSymbol);
                        }
                        continue;
                    case TypeKind.Enum:
                        var underlyingTypeSymbol = ((INamedTypeSymbol)currentSymbol).EnumUnderlyingType;
                        if (underlyingTypeSymbol != null && !visited.Contains(underlyingTypeSymbol))
                        {
                            stack.Push(underlyingTypeSymbol);
                            visited.Add(underlyingTypeSymbol);
                        }
                        continue;
                    case TypeKind.Class:
                    case TypeKind.Struct:
                        var namedTypeSymbol = currentSymbol as INamedTypeSymbol;
                        var arity = namedTypeSymbol?.Arity;
                        var args = namedTypeSymbol?.TypeArguments ?? ImmutableArray.Create<ITypeSymbol>();
                        var symbol = (arity.HasValue && arity.Value > 0) ? currentSymbol.OriginalDefinition : currentSymbol;

                        if (supportedValueTypesForSerilization.Contains(symbol) || supportedReferenceTypesForSerialization.Contains(symbol))
                        {
                            // Type is either primitive and/or directly supported by DotVVM
                            foreach (var arg in args)
                            {
                                if (!visited.Contains(arg))
                                {
                                    stack.Push(arg);
                                    visited.Add(arg);
                                }
                            }
                        }
                        else if (!currentSymbol.ContainingNamespace.ToDisplayString().StartsWith("System"))
                        {
                            // User types are supported if all their properties are supported
                            foreach (var property in symbol.GetMembers().Where(m => m.Kind == SymbolKind.Property).Cast<IPropertySymbol>())
                            {
                                if (!visited.Contains(property.Type))
                                {
                                    stack.Push(property.Type);
                                    visited.Add(property.Type);
                                }
                            }
                        }
                        else
                        {
                            // Something unsupported from BCL detected
                            return false;
                        }
                        continue;
                    default:
                        return false;
                }
            }

            return true;
        }

        private static void CreateTypesCache(Compilation compilation)
        {
            var valueTypeBuilder = ImmutableHashSet.CreateBuilder<ISymbol>(SymbolEqualityComparer.Default);
            var referenceTypeBuilder = ImmutableHashSet.CreateBuilder<ISymbol>(SymbolEqualityComparer.Default);

            // Supported reference types (excluding user-defined types)
            referenceTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_Object));
            referenceTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_String));
            referenceTypeBuilder.Add(compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")!);
            referenceTypeBuilder.Add(compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2")!);

            // Whitelisted DotVVM types commonly found in viewmodels
            referenceTypeBuilder.Add(compilation.GetTypeByMetadataName("DotVVM.Framework.Controls.GridViewDataSet`1")!);
            referenceTypeBuilder.Add(compilation.GetTypeByMetadataName("DotVVM.Framework.Controls.UploadedFilesCollection")!);

            // Common value types: (primitives)
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_Boolean));
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_Byte));
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_SByte));
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_Int16));
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_UInt16));
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_Int32));
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_UInt32));
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_Int64));
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_UInt64));
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_Single));
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_Double));
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_Decimal));
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_Char));

            // Special (whitelisted) value types
            valueTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_DateTime));
            valueTypeBuilder.Add(compilation.GetTypeByMetadataName("System.Guid")!);
            valueTypeBuilder.Add(compilation.GetTypeByMetadataName("System.TimeSpan")!);

            supportedValueTypesForSerilization = valueTypeBuilder.ToImmutableHashSet(SymbolEqualityComparer.Default);
            supportedReferenceTypesForSerialization = referenceTypeBuilder.ToImmutableHashSet(SymbolEqualityComparer.Default);
            cachesConstructed = true;
        }
    }
}
