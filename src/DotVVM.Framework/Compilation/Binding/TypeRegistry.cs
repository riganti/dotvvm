﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Binding
{
    public class TypeRegistry
    {
        private readonly CompiledAssemblyCache compiledAssemblyCache;
        private readonly ImmutableDictionary<string, Expression> registry;
        private readonly ImmutableList<Func<string, Expression>> resolvers;

        public TypeRegistry(CompiledAssemblyCache compiledAssemblyCache, ImmutableDictionary<string, Expression> registry, ImmutableList<Func<string, Expression>> resolvers)
        {
            this.compiledAssemblyCache = compiledAssemblyCache;
            this.registry = registry;
            this.resolvers = resolvers;
        }

        public Expression Resolve(string name, bool throwOnNotFound = true)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                if (throwOnNotFound) throw new ArgumentException($"The identifier name was empty.", nameof(name));
                else return null;
            }

            Expression expr;
            if (registry.TryGetValue(name, out expr))
            {
                return expr;
            }
            expr = resolvers.Select(r => r(name)).FirstOrDefault(e => e != null);
            if (expr != null) return expr;
            if (throwOnNotFound) throw new InvalidOperationException($"The identifier '{ name }' could not be resolved!");
            return null;
        }

        public TypeRegistry AddSymbols(IEnumerable<ParameterExpression> symbols) =>
            AddSymbols(symbols.Select(s => new KeyValuePair<string, Expression>(s.Name, s)));

        public TypeRegistry AddSymbols(IEnumerable<KeyValuePair<string, Expression>> symbols)
        {
            return new TypeRegistry(compiledAssemblyCache, registry.AddRange(symbols), resolvers);
        }

        public TypeRegistry AddSymbols(IEnumerable<Func<string, Expression>> symbols)
        {
            return new TypeRegistry(compiledAssemblyCache, registry, resolvers.InsertRange(0, symbols));
        }

        public static Expression CreateStatic(Type type)
        {
            return type == null ? null : new StaticClassIdentifierExpression(type);
        }

        public static TypeRegistry Default(CompiledAssemblyCache compiledAssemblyCache) => new TypeRegistry(compiledAssemblyCache,
            ImmutableDictionary<string, Expression>.Empty
                .Add("object", CreateStatic(typeof(Object)))
                .Add("bool", CreateStatic(typeof(Boolean)))
                .Add("byte", CreateStatic(typeof(Byte)))
                .Add("char", CreateStatic(typeof(Char)))
                .Add("short", CreateStatic(typeof(Int16)))
                .Add("int", CreateStatic(typeof(Int32)))
                .Add("long", CreateStatic(typeof(Int64)))
                .Add("ushort", CreateStatic(typeof(UInt16)))
                .Add("uint", CreateStatic(typeof(UInt32)))
                .Add("ulong", CreateStatic(typeof(UInt64)))
                .Add("decimal", CreateStatic(typeof(Decimal)))
                .Add("double", CreateStatic(typeof(Double)))
                .Add("float", CreateStatic(typeof(Single)))
                .Add("string", CreateStatic(typeof(String)))
                .Add("Object", CreateStatic(typeof(Object)))
                .Add("Boolean", CreateStatic(typeof(Boolean)))
                .Add("Byte", CreateStatic(typeof(Byte)))
                .Add("Char", CreateStatic(typeof(Char)))
                .Add("Int16", CreateStatic(typeof(Int16)))
                .Add("Int32", CreateStatic(typeof(Int32)))
                .Add("Int64", CreateStatic(typeof(Int64)))
                .Add("UInt16", CreateStatic(typeof(UInt16)))
                .Add("UInt32", CreateStatic(typeof(UInt32)))
                .Add("UInt64", CreateStatic(typeof(UInt64)))
                .Add("Decimal", CreateStatic(typeof(Decimal)))
                .Add("Double", CreateStatic(typeof(Double)))
                .Add("Single", CreateStatic(typeof(Single)))
                .Add("String", CreateStatic(typeof(String))),
            ImmutableList<Func<string, Expression>>.Empty
                .Add(type => CreateStatic(compiledAssemblyCache.FindType(type)))
                .Add(type => CreateStatic(compiledAssemblyCache.FindType("System." + type)))
            );

        public static TypeRegistry DirectivesDefault(CompiledAssemblyCache compiledAssemblyCache, string assemblyName = null) => new TypeRegistry(compiledAssemblyCache,
           ImmutableDictionary<string, Expression>.Empty
               .Add("object", CreateStatic(typeof(Object)))
               .Add("bool", CreateStatic(typeof(Boolean)))
               .Add("byte", CreateStatic(typeof(Byte)))
               .Add("char", CreateStatic(typeof(Char)))
               .Add("short", CreateStatic(typeof(Int16)))
               .Add("int", CreateStatic(typeof(Int32)))
               .Add("long", CreateStatic(typeof(Int64)))
               .Add("ushort", CreateStatic(typeof(UInt16)))
               .Add("uint", CreateStatic(typeof(UInt32)))
               .Add("ulong", CreateStatic(typeof(UInt64)))
               .Add("decimal", CreateStatic(typeof(Decimal)))
               .Add("double", CreateStatic(typeof(Double)))
               .Add("float", CreateStatic(typeof(Single)))
               .Add("string", CreateStatic(typeof(String)))
               .Add("Object", CreateStatic(typeof(Object)))
               .Add("Boolean", CreateStatic(typeof(Boolean)))
               .Add("Byte", CreateStatic(typeof(Byte)))
               .Add("Char", CreateStatic(typeof(Char)))
               .Add("Int16", CreateStatic(typeof(Int16)))
               .Add("Int32", CreateStatic(typeof(Int32)))
               .Add("Int64", CreateStatic(typeof(Int64)))
               .Add("UInt16", CreateStatic(typeof(UInt16)))
               .Add("UInt32", CreateStatic(typeof(UInt32)))
               .Add("UInt64", CreateStatic(typeof(UInt64)))
               .Add("Decimal", CreateStatic(typeof(Decimal)))
               .Add("Double", CreateStatic(typeof(Double)))
               .Add("Single", CreateStatic(typeof(Single)))
               .Add("String", CreateStatic(typeof(String))),
           ImmutableList<Func<string, Expression>>.Empty
               .Add(type => CreateStatic(compiledAssemblyCache.FindType(type + (assemblyName != null ? $", {assemblyName}" : ""))))
           );
    }
}
