using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Symbols
{
    public static class SymbolExtensions
    {
        public static bool HasAttribute<T>(this ISymbol symbol)
        {
            var attributeType = typeof(T);
            return symbol.GetAttributes().Any(a => a.AttributeClass.ToString().Equals(attributeType.FullName));
        }
    }
}
