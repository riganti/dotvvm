using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public static class TsSyntaxExtensions
    {
        public static string ToDisplayString(this TsModifier modifier)
        {
            switch (modifier)
            {
                case TsModifier.Public:
                    return "public";
                case TsModifier.Private:
                    return "private";
                case TsModifier.Protected:
                    return "protected";
                default:
                    return string.Empty;
            }
        }
    }
}
