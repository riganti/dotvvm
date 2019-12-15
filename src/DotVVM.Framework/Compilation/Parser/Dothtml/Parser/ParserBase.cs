using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public abstract class ParserBase<TToken, TTokenType> where TToken : TokenBase<TTokenType>
    {
        /// <summary>
        /// Gets the tokens from.
        /// </summary>
        protected AggregateList<TToken>.Part GetTokensFrom(int startIndex)
        {
            return new AggregateList<TToken>.Part(Tokens, startIndex, CurrentIndex - startIndex); // Enumerable.Skip<TToken>(Tokens, startIndex).Take(CurrentIndex - startIndex);
        }

        protected abstract bool IsWhiteSpace(TToken token);

        public List<TToken> Tokens { get; set; }
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
        public TToken Peek()
        {
            if (CurrentIndex < Tokens.Count)
            {
                return Tokens[CurrentIndex];
            }
            return null;
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