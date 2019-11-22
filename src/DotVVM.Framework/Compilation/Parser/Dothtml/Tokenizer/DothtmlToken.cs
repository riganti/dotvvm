namespace DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer
{
    public class DothtmlToken : TokenBase<DothtmlTokenType>
    {
        public DothtmlToken(string text, DothtmlTokenType type, int lineNumber, int columnNumber, int length, int startPosition) : base(text, type, lineNumber, columnNumber, length, startPosition)
        {
        }
    }
}
