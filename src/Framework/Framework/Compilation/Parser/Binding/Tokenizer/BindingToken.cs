namespace DotVVM.Framework.Compilation.Parser.Binding.Tokenizer
{
    public class BindingToken : TokenBase<BindingTokenType>
    {
        public BindingToken(string text, BindingTokenType type, int lineNumber, int columnNumber, int length, int startPosition) : base(text, type, lineNumber, columnNumber, length, startPosition)
        {
        }

        /// <summary> Returns new token with its position changed relative to the provided binding value token </summary>
        public BindingToken RemapPosition(TokenBase parentToken) =>
            RemapPosition(parentToken.LineNumber, parentToken.ColumnNumber, parentToken.StartPosition);
        /// <summary> Returns new token with its position changed relative to the provided binding start position </summary>
        public BindingToken RemapPosition(int startLine, int startColumn, int startPosition)
        {
            return new BindingToken(Text, Type,
                startLine + this.LineNumber - 1,
                this.LineNumber <= 1 ? startColumn + this.ColumnNumber : this.ColumnNumber,
                Length,
                this.StartPosition + startPosition);
        }
    }
}
