using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Resources;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public abstract class ParserBase<TToken, TTokenType> where TToken : TokenBase<TTokenType>
    {
        /// <summary>
        /// Gets the tokens from.
        /// </summary>
        protected IEnumerable<TToken> GetTokensFrom(int startIndex)
        {
            return Enumerable.Skip<TToken>(Tokens, startIndex).Take(CurrentIndex - startIndex);
        }

        protected abstract TTokenType WhiteSpaceToken { get; }

        internal IList<TToken> Tokens { get; set; }
        protected int CurrentIndex { get; set; }

        
        /// <summary>
        /// Skips the whitespace.
        /// </summary>
        protected List<TToken> SkipWhiteSpace()
        {
            return Enumerable.ToList<TToken>(ReadMultiple(t => t.Type.Equals(WhiteSpaceToken)));
        }

        /// <summary>
        /// Peeks the current token.
        /// </summary>
        public TToken Peek()
        {
            if (CurrentIndex < Tokens.Count)
            {
                return Tokens[CurrentIndex];
            }
            return null;
        }

        /// <summary>
        /// Reads the current token and advances to the next one.
        /// </summary>
        public TToken Read()
        {
            if (CurrentIndex < Tokens.Count)
            {
                return Tokens[CurrentIndex++];
            }
            return null;
        }

        /// <summary>
        /// Reads the current token and advances to the next one.
        /// </summary>
        public IEnumerable<TToken> ReadMultiple(Func<TToken, bool> filter)
        {
            var current = Peek();
            while (current != null && filter(current))
            {
                yield return current;
                Read();
                current = Peek();
            }
        }
    }
}