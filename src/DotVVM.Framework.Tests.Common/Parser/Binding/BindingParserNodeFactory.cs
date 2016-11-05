using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;

namespace DotVVM.Framework.Tests.Parser.Binding
{
    public class BindingParserNodeFactory
    {
        public BindingParserNode Parse(string expression)
        {
            BindingParser parser = SetupParser(expression);
            return parser.ReadExpression();
        }

        public BindingParser SetupParser(string expression)
        {
            var tokenizer = new BindingTokenizer();
            tokenizer.Tokenize(expression);
            var parser = new BindingParser();
            parser.Tokens = tokenizer.Tokens;
            return parser;
        }
    }
}