using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Analyzers.Serializability
{
    internal static class TypeSymbolExtensions
    {
        private static readonly ConditionalWeakTable<Compilation, ImmutableHashSet<ISymbol>> symbolsCache = new();

        public static bool IsSerializationSupported(this ITypeSymbol typeSymbol, Compilation compilation, out string? errorPath)
        {
            return IsSerializationSupportedImpl(typeSymbol, compilation, out errorPath);
        }

        public static bool IsEnumerable(this ITypeSymbol typeSymbol, Compilation compilation)
        {
            var enumerable = compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);
            return typeSymbol.AllInterfaces.Contains(enumerable);
        }

        private static ISet<ISymbol> GetSymbolsCache(Compilation compilation)
        {
            // Fast path - cache is already constructed
            if (!symbolsCache.TryGetValue(compilation, out var hashset))
            {
                // Slow path - cache needs to be constructed
                lock (compilation)
                {
                    // Ensure cache for each compilation is constructed at most once
                    if (!symbolsCache.TryGetValue(compilation, out hashset))
                    {
                        hashset = CreateTypesCache(compilation);
                        symbolsCache.Add(compilation, hashset);
                    }
                }
            }

            return hashset;
        }

        private static bool IsSerializationSupportedImpl(this ITypeSymbol typeSymbol, Compilation compilation, out string? errorPath)
        {
            var cache = GetSymbolsCache(compilation);
            var stack = new Stack<(ITypeSymbol Symbol, string Path)>();
#pragma warning disable RS1024 // Compare symbols correctly
            // This is a false positive: https://github.com/dotnet/roslyn-analyzers/issues/4568
            var visited = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default) { typeSymbol };
#pragma warning restore RS1024 // Compare symbols correctly
            stack.Push((typeSymbol, string.Empty));

            while (stack.Count != 0)
            {
                var (currentSymbol, currentPath) = stack.Pop();
                switch (currentSymbol.TypeKind)
                {
                    case TypeKind.Array:
                        var elementTypeSymbol = ((IArrayTypeSymbol)currentSymbol).ElementType;
                        if (!visited.Contains(elementTypeSymbol))
                        {
                            stack.Push((elementTypeSymbol, currentPath));
                            visited.Add(elementTypeSymbol);
                        }
                        continue;
                    case TypeKind.Enum:
                        var underlyingTypeSymbol = ((INamedTypeSymbol)currentSymbol).EnumUnderlyingType;
                        if (underlyingTypeSymbol != null && !visited.Contains(underlyingTypeSymbol))
                        {
                            stack.Push((underlyingTypeSymbol, currentPath));
                            visited.Add(underlyingTypeSymbol);
                        }
                        continue;
                    case TypeKind.Interface:
                        if (currentSymbol.IsEnumerable(compilation))
                        {
                            foreach (var arg in (currentSymbol as INamedTypeSymbol)!.TypeArguments)
                            {
                                stack.Push((arg, currentPath));
                                visited.Add(arg);
                            }
                        }
                        else
                        {
                            errorPath = currentPath;
                            return false;
                        }
                        continue;
                    case TypeKind.Class:
                    case TypeKind.Struct:
                        // Unwrap nullables
                        if (currentSymbol.NullableAnnotation == NullableAnnotation.Annotated)
                            currentSymbol = (currentSymbol as INamedTypeSymbol)!.TypeArguments.First();

                        var namedTypeSymbol = currentSymbol as INamedTypeSymbol;
                        var arity = namedTypeSymbol?.Arity;
                        var args = namedTypeSymbol?.TypeArguments ?? ImmutableArray.Create<ITypeSymbol>();
                        var originalDefinitionSymbol = (arity.HasValue && arity.Value > 0) ? currentSymbol.OriginalDefinition : currentSymbol;

                        if (cache.Contains(originalDefinitionSymbol) || originalDefinitionSymbol.IsEnumerable(compilation))
                        {
                            // Type is either primitive and/or directly supported by DotVVM
                            foreach (var arg in args)
                            {
                                if (!visited.Contains(arg))
                                {
                                    stack.Push((arg, currentPath));
                                    visited.Add(arg);
                                }
                            }
                        }
                        else if (!currentSymbol.ContainingNamespace.ToDisplayString().StartsWith("System"))
                        {
                            // User types are supported if all their properties are supported
                            foreach (var property in currentSymbol.GetMembers().Where(m => m.Kind == SymbolKind.Property).Cast<IPropertySymbol>())
                            {
                                if (!visited.Contains(property.Type))
                                {
                                    stack.Push((property.Type, $"{currentPath}.{property.Name}"));
                                    visited.Add(property.Type);
                                }
                            }
                        }
                        else
                        {
                            // Something unsupported from BCL detected
                            errorPath = currentPath;
                            return false;
                        }
                        continue;
                    default:
                        errorPath = currentPath;
                        return false;
                }
            }

            errorPath = null;
            return true;
        }

        private static ImmutableHashSet<ISymbol> CreateTypesCache(Compilation compilation)
        {
            var cacheBuilder = ImmutableHashSet.CreateBuilder<ISymbol>(SymbolEqualityComparer.Default);

            // Supported reference types (excluding user-defined types)
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_Object));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_String));
            cacheBuilder.Add(compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")!);
            cacheBuilder.Add(compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2")!);

            // Whitelisted DotVVM types commonly found in viewmodels
            cacheBuilder.Add(compilation.GetTypeByMetadataName("DotVVM.Framework.Controls.GridViewDataSet`1")!);
            cacheBuilder.Add(compilation.GetTypeByMetadataName("DotVVM.Framework.Controls.UploadedFilesCollection")!);
            var bpGridViewDataSet = compilation.GetTypeByMetadataName("DotVVM.BusinessPack.Controls.BusinessPackDataSet`1");
            if (bpGridViewDataSet != null)
                cacheBuilder.Add(bpGridViewDataSet);

            // Common value types: (primitives)
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_Boolean));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_Byte));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_SByte));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_Int16));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_UInt16));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_Int32));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_UInt32));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_Int64));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_UInt64));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_Single));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_Double));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_Decimal));
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_Char));

            // Special (whitelisted) value types
            cacheBuilder.Add(compilation.GetSpecialType(SpecialType.System_DateTime));
            cacheBuilder.Add(compilation.GetTypeByMetadataName("System.Guid")!);
            cacheBuilder.Add(compilation.GetTypeByMetadataName("System.TimeSpan")!);
            cacheBuilder.Add(compilation.GetTypeByMetadataName("System.DateTimeOffset")!);

            return cacheBuilder.ToImmutableHashSet(SymbolEqualityComparer.Default);
        }
    }
}
