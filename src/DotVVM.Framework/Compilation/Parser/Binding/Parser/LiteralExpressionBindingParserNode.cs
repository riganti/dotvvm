using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LiteralExpressionBindingParserNode : BindingParserNode
    {
        public object? Value { get; set; }

        public LiteralExpressionBindingParserNode(object? value)
        {
            Value = value;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => Enumerable.Empty<BindingParserNode>();

        public override string ToDisplayString()
            => Value is null ? "<null>" : Value.ToString()!;
    }
}
