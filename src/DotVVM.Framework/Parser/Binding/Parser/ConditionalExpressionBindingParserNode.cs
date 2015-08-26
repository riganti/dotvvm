using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser.Binding.Parser
{
    public class ConditionalExpressionBindingParserNode : BindingParserNode
    {
        public BindingParserNode ConditionExpression { get; private set; }
        public BindingParserNode TrueExpression { get; private set; }
        public BindingParserNode FalseExpression { get; private set; }

        public ConditionalExpressionBindingParserNode(BindingParserNode conditionExpression, BindingParserNode trueExpression, BindingParserNode falseExpression)
        {
            ConditionExpression = conditionExpression;
            TrueExpression = trueExpression;
            FalseExpression = falseExpression;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            return
                base.EnumerateNodes()
                    .Concat(ConditionExpression.EnumerateNodes())
                    .Concat(TrueExpression.EnumerateNodes())
                    .Concat(FalseExpression.EnumerateNodes());
        }
    }
}