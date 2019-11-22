#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class BlockBindingParserNode : BindingParserNode
    {
        public BindingParserNode FirstExpression { get; }
        public BindingParserNode SecondExpression { get; }

        public BlockBindingParserNode(BindingParserNode firstExpression, BindingParserNode secondExpression)
        {
            this.FirstExpression = firstExpression;
            this.SecondExpression = secondExpression;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new [] { FirstExpression, SecondExpression };

        public override string ToDisplayString()
            => $"{FirstExpression.ToDisplayString()}; {SecondExpression.ToDisplayString()}";
    }
}
