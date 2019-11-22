#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FunctionCallBindingParserNode : BindingParserNode
    {
        public BindingParserNode TargetExpression { get; private set; }
        public List<BindingParserNode> ArgumentExpressions { get; private set; }

        public FunctionCallBindingParserNode(BindingParserNode targetExpression, List<BindingParserNode> argumentExpressions)
        {
            TargetExpression = targetExpression;
            ArgumentExpressions = argumentExpressions;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            return
                base.EnumerateNodes()
                    .Concat(TargetExpression.EnumerateNodes())
                    .Concat(ArgumentExpressions.SelectMany(a => a.EnumerateNodes()));
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { TargetExpression }.Concat(ArgumentExpressions);

        public override string ToDisplayString() => $"{TargetExpression.ToDisplayString()}({string.Join(", ", ArgumentExpressions.Select(e => e.ToDisplayString()))})";
    }
}
