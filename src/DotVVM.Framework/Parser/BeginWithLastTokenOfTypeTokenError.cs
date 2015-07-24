using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser
{
    /// <summary>
    /// Represents a range defined by the last token and type of the token where the error starts.
    /// </summary>
    public class BeginWithLastTokenOfTypeTokenError<TToken, TTokenType> : TokenError<TToken, TTokenType> where TToken : TokenBase<TTokenType>, new()
    {
        public TToken LastToken { get; private set; }

        public TTokenType FirstTokenType { get; private set; }

        public BeginWithLastTokenOfTypeTokenError(string errorMessage, TokenizerBase<TToken, TTokenType> tokenizer, TToken lastToken, TTokenType firstTokenType) : base(errorMessage, tokenizer)
        {
            LastToken = lastToken;
            FirstTokenType = firstTokenType;
        }

        protected override TextRange GetRange()
        {
            var tokenIndex = Tokenizer.Tokens.IndexOf(LastToken);
            do
            {
                tokenIndex--;
            } while (tokenIndex >= 0 && Tokenizer.Tokens[tokenIndex].Type.Equals(FirstTokenType));

            var begin = tokenIndex >= 0 ? Tokenizer.Tokens[tokenIndex].StartPosition : 0;
            var end = LastToken.StartPosition + LastToken.Length;
            return new TextRange(begin, end - begin);
        }
    }
}