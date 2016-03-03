namespace DotVVM.Framework.Compilation.Parser
{
    public class SimpleTokenError<TToken, TTokenType> : TokenError<TToken, TTokenType> where TToken : TokenBase<TTokenType>, new()
    {
        public TToken Token { get; private set; }

        public SimpleTokenError(string errorMessage, TokenizerBase<TToken, TTokenType> tokenizer, TToken token) : base(errorMessage, tokenizer)
        {
            Token = token;
        }

        protected override TextRange GetRange()
        {
            return new TextRange(Token.StartPosition, Token.Length);
        }
    }
}