#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser
{
    public static class TokenListExtensions
    {
        public static int SumTokenLength<TToken>(this AggregateList<TToken> tokens)
            where TToken: TokenBase
        {
            var sum = 0;
            foreach (var t in tokens)
            {
                sum += t.Length;
            }
            return sum;
        }

        public static string ConcatTokenText<TToken>(this AggregateList<TToken> tokens)
            where TToken: TokenBase
        {
            if (tokens.TryGetSinglePart(out var part))
            {
                // zero allocation when empty or singleton
                if (part.len == 0) return string.Empty;
                else if (part.len == 1) return part.list[part.from].Text;
            }
            // this is faster than linq, tokens are passed by argument to prevent display-class allocation
            IEnumerable<string> enumText(AggregateList<TToken> tt)
            {
                foreach (var t in tt)
                    yield return t.Text;
            }
            return string.Concat(enumText(tokens));
        }
    }
}
