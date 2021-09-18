using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace DotVVM.Analysers.Serializability
{
    internal static class TypeSymbolExtensions
    {
        private static ImmutableList<ITypeSymbol> supportedTypesForSerialization = ImmutableList.Create<ITypeSymbol>();

        public static bool IsPrimitive(this ITypeSymbol typeSymbol)
        {
            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_Byte:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Decimal:
                case SpecialType.System_Char:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSerializationSupported(this ITypeSymbol typeSymbol, SemanticModel semantics)
        {
            if (supportedTypesForSerialization.Count == 0)
                CreateTypesCache(semantics.Compilation);

            return typeSymbol.IsSerializationSupportedImpl();
        }

        public static bool IsSerializationSupportedImpl(this ITypeSymbol typeSymbol)
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

                    if (symbol.IsPrimitive() || supportedTypesForSerialization.Contains(symbol))
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
            var builder = ImmutableArray.CreateBuilder<ITypeSymbol>();

            builder.Add(compilation.GetSpecialType(SpecialType.System_Object));
            builder.Add(compilation.GetSpecialType(SpecialType.System_String));
            builder.Add(compilation.GetSpecialType(SpecialType.System_DateTime));
            builder.Add(compilation.GetTypeByMetadataName("System.Guid"));
            builder.Add(compilation.GetTypeByMetadataName("System.TimeSpan"));
            builder.Add(compilation.GetTypeByMetadataName("System.Collections.Generic.List`1"));
            builder.Add(compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2"));

            supportedTypesForSerialization = builder.ToImmutableList();
        }
    }
}
