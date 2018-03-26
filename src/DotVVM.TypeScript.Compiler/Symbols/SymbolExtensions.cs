using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast;
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

        public static TsModifier ToTsModifier(this Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.Private:
                    return TsModifier.Private;
                case Accessibility.Protected:
                    return TsModifier.Protected;
                case Accessibility.Public:
                default:
                    return TsModifier.Public;
            }
        }
    }
}
