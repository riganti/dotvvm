namespace DotVVM.Framework.Compilation.Parser
{
    public abstract class TokenBase : ITextRange
    {
        protected TokenBase(string text, int lineNumber, int columnNumber, int length, int startPosition)
        {
            Text = text ?? throw new System.ArgumentNullException(nameof(text));
            StartPosition = startPosition;
            Length = length;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public int StartPosition { get; set; }

        public int Length { get; set; }

        public int EndPosition => StartPosition + Length;

        public string Text { get; set; }

        public int LineNumber { get; set; }

        public int ColumnNumber { get; set; }

        public TokenError? Error { get; set; }

        public bool HasError => Error is not null;
        public override string ToString()
        {
            return string.Format("Token ({0}:{1}): {2}", StartPosition, Length, Text);
        }
    }

    public abstract class TokenBase<TTokenType> : TokenBase
    {
        protected TokenBase(string text, TTokenType type, int lineNumber, int columnNumber, int length, int startPosition)
            : base(text, lineNumber, columnNumber, length, startPosition)
        {
            this.Type = type;
        }

        public TTokenType Type { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1}:{2}): {3}", Type, StartPosition, Length, Text);
        }
    }
}
