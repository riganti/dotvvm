namespace DotVVM.Framework.Compilation.Parser
{
    public class NullTokenError<TToken, TTokenType> : TokenError<TToken, TTokenType> where TToken : TokenBase<TTokenType>
    {
        public NullTokenError(TokenizerBase<TToken, TTokenType> tokenizer) : base("", tokenizer)
        {
        }

        protected override TextRange? GetRange()
        {
            return null;
        }
    }
}
