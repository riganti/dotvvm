using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser.Binding.Parser
{
    public class LiteralExpressionBindingParserNode : BindingParserNode
    {
        public object Value { get; set; }

        public LiteralExpressionBindingParserNode(object value)
        {
            Value = value;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => Enumerable.Empty<BindingParserNode>();
    }
}