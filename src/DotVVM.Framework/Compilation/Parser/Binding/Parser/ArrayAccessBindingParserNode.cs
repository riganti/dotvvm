using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    public class ArrayAccessBindingParserNode : BindingParserNode
    {
        public BindingParserNode TargetExpression { get; private set; }
        public BindingParserNode ArrayIndexExpression { get; private set; }

        public ArrayAccessBindingParserNode(BindingParserNode targetExpression, BindingParserNode arrayIndexExpression)
        {
            TargetExpression = targetExpression;
            ArrayIndexExpression = arrayIndexExpression;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(TargetExpression.EnumerateNodes()).Concat(ArrayIndexExpression.EnumerateNodes());
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { TargetExpression, ArrayIndexExpression };
    }
}