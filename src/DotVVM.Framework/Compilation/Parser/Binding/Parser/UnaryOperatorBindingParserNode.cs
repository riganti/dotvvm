#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class UnaryOperatorBindingParserNode : BindingParserNode
    {
        public BindingParserNode InnerExpression { get; private set; }
        public BindingTokenType Operator { get; private set; }

        public UnaryOperatorBindingParserNode(BindingParserNode innerExpression, BindingTokenType @operator)
        {
            InnerExpression = innerExpression;
            Operator = @operator;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(InnerExpression.EnumerateNodes());
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { InnerExpression };

        public override string ToDisplayString()
            => $"{Operator.ToDisplayString()}{InnerExpression}";
    }
}
