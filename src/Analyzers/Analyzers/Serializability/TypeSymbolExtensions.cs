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

        public static bool IsKnownSerializableType(this ITypeSymbol typeSymbol, Compilation compilation)
        {
            var cache = GetSymbolsCache(compilation);
            return cache.Contains(typeSymbol);
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
