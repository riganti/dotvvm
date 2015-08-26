using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Binding.Tokenizer;

namespace DotVVM.Framework.Parser.Binding.Parser
{
    public class BinaryOperatorBindingParserNode : BindingParserNode
    {
        public BindingParserNode FirstExpression { get; private set; }
        public BindingParserNode SecondExpression { get; private set; }
        public BindingTokenType Operator { get; private set; }

        public BinaryOperatorBindingParserNode(BindingParserNode firstExpression, BindingParserNode secondExpression, BindingTokenType @operator)
        {
            FirstExpression = firstExpression;
            SecondExpression = secondExpression;
            Operator = @operator;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(FirstExpression.EnumerateNodes()).Concat(SecondExpression.EnumerateNodes());
        }
    }
}