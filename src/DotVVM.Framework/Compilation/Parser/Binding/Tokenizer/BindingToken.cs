#nullable enable
namespace DotVVM.Framework.Compilation.Parser.Binding.Tokenizer
{
    public class BindingToken : TokenBase<BindingTokenType>
    {
        public BindingToken(string text, BindingTokenType type, int lineNumber, int columnNumber, int length, int startPosition) : base(text, type, lineNumber, columnNumber, length, startPosition)
        {
        }
    }
}
