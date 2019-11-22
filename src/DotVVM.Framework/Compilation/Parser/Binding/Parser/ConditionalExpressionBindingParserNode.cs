#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
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

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { ConditionExpression, TrueExpression, FalseExpression };

        public override string ToDisplayString() => $"{ConditionExpression.ToDisplayString()}?{TrueExpression.ToDisplayString()}:{FalseExpression.ToDisplayString()}";
    }
}
