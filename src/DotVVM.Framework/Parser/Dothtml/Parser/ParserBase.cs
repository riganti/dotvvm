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
        /// Asserts that the current token is of a specified type.
        /// </summary>
        protected void Assert(TTokenType desiredType)
        {
            if (Peek() == null || !Peek().Type.Equals(desiredType))
            {
                throw new Exception("Assertion failed! This is internal error of the parser.");
            }
        }

        /// <summary>
        /// Skips the whitespace.
        /// </summary>
        protected List<TToken> SkipWhitespace()
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
            throw new ParserException(Parser_Dothtml.UnexpectedEndOfInput);
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