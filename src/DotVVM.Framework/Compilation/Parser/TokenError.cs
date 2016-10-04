namespace DotVVM.Framework.Compilation.Parser
{
	public abstract class TokenError
	{
		public string ErrorMessage { get; private set; }

		public abstract ITextRange Range { get; }

		public bool IsCritical { get; }

		protected TokenError(string errorMessage, bool isCritical)
		{
			ErrorMessage = errorMessage;
			IsCritical = isCritical;
		}
	}

	public abstract class TokenError<TToken, TTokenType> : TokenError where TToken : TokenBase<TTokenType>, new()
	{
		public TokenizerBase<TToken, TTokenType> Tokenizer { get; private set; }

		private TextRange range = null;
		public override ITextRange Range => range ?? (range = GetRange());

		public TokenError(string errorMessage, TokenizerBase<TToken, TTokenType> tokenizer, bool isCritical = false) : base(errorMessage, isCritical)
		{
			Tokenizer = tokenizer;
		}

		protected abstract TextRange GetRange();
	}
}