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
        /// <summary> When not null, the result of the first expression is assigned to a variable with the specified name. </summary>
        public IdentifierNameBindingParserNode? Variable { get; }

        public BlockBindingParserNode(BindingParserNode firstExpression, BindingParserNode secondExpression, IdentifierNameBindingParserNode? variable = null)
        {
            this.FirstExpression = firstExpression;
            this.SecondExpression = secondExpression;
            this.Variable = variable;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new [] { FirstExpression, SecondExpression };

        public override string ToDisplayString()
            => (Variable is object ? $"var {Variable.Name} = " : "") + $"{FirstExpression.ToDisplayString()}; {SecondExpression.ToDisplayString()}";
    }
}
