using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Parser;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{
    public class CompileTimeBindingParser : IBindingParser
    {

        public Expression Parse(string expression, DataContextStack dataContexts)
        {
            var tokenizer = new Parser.Binding.Tokenizer.BindingTokenizer();
            tokenizer.Tokenize(new StringReader(expression));

            var parser = new Parser.Binding.Parser.BindingParser();
            parser.Tokens = tokenizer.Tokens;
            var node = parser.ReadExpression();

            var visitor = new DataContextResolverBindingParserNodeVisitor();
        }

    }
}
