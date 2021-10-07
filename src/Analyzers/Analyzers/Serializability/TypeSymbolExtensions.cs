using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Analyzers.Serializability
{
    internal static class TypeSymbolExtensions
    {
        private static ImmutableHashSet<ITypeSymbol> supportedReferenceTypesForSerialization = ImmutableHashSet.Create<ITypeSymbol>();
        private static ImmutableHashSet<ITypeSymbol> supportedValueTypesForSerilization = ImmutableHashSet.Create<ITypeSymbol>();
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
            switch (typeSymbol.TypeKind)
            {
                case TypeKind.Array:
                    return ((IArrayTypeSymbol)typeSymbol).ElementType.IsSerializationSupportedImpl();
                case TypeKind.Enum:
                    return ((INamedTypeSymbol)typeSymbol).EnumUnderlyingType.IsSerializationSupportedImpl();
                case TypeKind.Class:
                case TypeKind.Struct:

                    var namedTypeSymbol = typeSymbol as INamedTypeSymbol;
                    var arity = namedTypeSymbol?.Arity;
                    var args = namedTypeSymbol?.TypeArguments ?? ImmutableArray.Create<ITypeSymbol>();
                    var symbol = (arity.HasValue && arity.Value > 0) ? typeSymbol.OriginalDefinition : typeSymbol;

                    if (supportedValueTypesForSerilization.Contains(symbol) || supportedReferenceTypesForSerialization.Contains(symbol))
                    {
                        // Type is either primitive and/or directly supported by DotVVM
                        foreach (var arg in args)
                        {
                            if (!arg.IsSerializationSupportedImpl())
                                return false;
                        }

                        return true;
                    }
                    else if (!typeSymbol.ContainingNamespace.ToDisplayString().StartsWith("System"))
                    {
                        // User types are supported if all their properties are supported
                        foreach (var property in symbol.GetMembers().Where(m => m.Kind == SymbolKind.Property).Cast<IPropertySymbol>())
                        {
                            if (!property.Type.IsSerializationSupportedImpl())
                                return false;
                        }

                        return true;
                    }

                    break;
            }

            return false;
        }

        private static void CreateTypesCache(Compilation compilation)
        {
            var valueTypeBuilder = ImmutableHashSet.CreateBuilder<ITypeSymbol>();
            var referenceTypeBuilder = ImmutableHashSet.CreateBuilder<ITypeSymbol>();

            // Supported reference types (excluding user-defined types)
            referenceTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_Object));
            referenceTypeBuilder.Add(compilation.GetSpecialType(SpecialType.System_String));
            referenceTypeBuilder.Add(compilation.GetTypeByMetadataName("System.Collections.Generic.List`1"));
            referenceTypeBuilder.Add(compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2"));

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
            valueTypeBuilder.Add(compilation.GetTypeByMetadataName("System.Guid"));
            valueTypeBuilder.Add(compilation.GetTypeByMetadataName("System.TimeSpan"));

            supportedValueTypesForSerilization = valueTypeBuilder.ToImmutableHashSet();
            supportedReferenceTypesForSerialization = referenceTypeBuilder.ToImmutableHashSet();
            cachesConstructed = true;
        }
    }
}
