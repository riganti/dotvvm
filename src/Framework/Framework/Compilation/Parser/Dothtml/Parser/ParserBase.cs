using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public abstract class ParserBase<TToken, TTokenType> where TToken : TokenBase<TTokenType>
                                                         where TTokenType : notnull
    {
        public TTokenType WhiteSpaceTokenType { get; }

        protected ParserBase(TTokenType whiteSpaceTokenType)
        {
            WhiteSpaceTokenType = whiteSpaceTokenType;
        }

        /// <summary>
        /// Gets the tokens from.
        /// </summary>
        protected AggregateList<TToken>.Part GetTokensFrom(int startIndex)
        {
            return new AggregateList<TToken>.Part(Tokens, startIndex, CurrentIndex - startIndex); // Enumerable.Skip<TToken>(Tokens, startIndex).Take(CurrentIndex - startIndex);
        }

        protected bool IsWhiteSpace(TToken token) =>
            EqualityComparer<TTokenType>.Default.Equals(this.WhiteSpaceTokenType, token.Type);

        public List<TToken> Tokens { get; set; } = new List<TToken>();
        protected int CurrentIndex { get; set; }
        
        /// <summary>
        /// Skips the whitespace.
        /// </summary>
        protected AggregateList<TToken>.Part SkipWhiteSpace()
        {
            var start = CurrentIndex;
            while(CurrentIndex < Tokens.Count && IsWhiteSpace(Tokens[CurrentIndex]))
            {
                Read();
            }
            return new AggregateList<TToken>.Part (Tokens, start, CurrentIndex - start);
        }

        /// <summary>
        /// Peeks the current token.
        /// </summary>
        [return: MaybeNull]
        public TToken Peek()
        {
            if (CurrentIndex < Tokens.Count)
            {
                return Tokens[CurrentIndex];
            }
            return null;
        }

        public TTokenType PeekType()
        {
            if (CurrentIndex < Tokens.Count)
            {
                return Tokens[CurrentIndex].Type;
            }
            return default!;
        }

        /// <summary>
        /// Peeks the current token, and throws an exception when token is not present.
        /// </summary>
        public TToken PeekOrFail() =>
            Peek() ?? throw new DotvvmCompilationException("Unexpected end of token stream", new [] { Tokens.Last() });


        /// <summary>
        /// Asserts that the current token is of a specified type.
        /// </summary>
        protected TToken Assert(TTokenType desiredType)
        {
            var token = Peek();
            if (token is null || !EqualityComparer<TTokenType>.Default.Equals(token.Type, desiredType))
            {
                throw new DotvvmCompilationException(
                    $"The token {desiredType} was expected! Instead found {(token == null ? "end of stream" : "" + token.Type)}",
                    new [] { token ?? Tokens.Last() });
            }
            return token;
        }

        private Stack<int> _restorePoints = new Stack<int>();
        protected void SetRestorePoint()
        {
            _restorePoints.Push(CurrentIndex);
        }

        protected void ClearRestorePoint()
        {
            if(_restorePoints.Count > 0)
            {
                _restorePoints.Pop();
            }
        }

        protected void Restore()
        {
            if(_restorePoints.Count != 0)
            {
                CurrentIndex = _restorePoints.Pop();
            }
        }

        protected AggregateList<TToken>.Part PeekPart()
        {
            return new AggregateList<TToken>.Part(Tokens, CurrentIndex, (CurrentIndex < Tokens.Count) ? 1: 0);
        }

        /// <summary>
        /// Reads the current token and advances to the next one.
        /// </summary>
        [return: MaybeNull]
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
