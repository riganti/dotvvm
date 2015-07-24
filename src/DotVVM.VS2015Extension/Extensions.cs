using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace DotVVM.VS2015Extension
{
    public static class Extensions
    {

        /// <summary>
        /// Replaces a range in a StringBuilder with specified content.
        /// </summary>
        public static void SetRange(this StringBuilder sb, int start, int length, string content)
        {
            length = Math.Min(content.Length, length);
            if (length < 0)
            {
                throw new ArgumentException("length");
            }

            for (int i = 0; i < length; i++)
            {
                if (start + i < sb.Length)
                {
                    sb[start + i] = content[i];
                }
                else
                {
                    sb.Append(content[i]);
                }
            }
        }

        public static IEnumerable<INamedTypeSymbol> GetThisAndAllBaseTypes(this INamedTypeSymbol symbol)
        {
            yield return symbol;
            while (symbol.BaseType != null)
            {
                yield return symbol.BaseType;
                symbol = symbol.BaseType;
            }
        }

    }
}
